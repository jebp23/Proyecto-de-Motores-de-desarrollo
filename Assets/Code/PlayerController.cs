using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private enum RotationAxes { MouseXAndY, MouseX, MouseY }

    [Header("References")]
    [SerializeField] private Camera _playerCamera; // Asignar en inspector

    // Sensibilidad y límites de rotación
    private const float MouseSensitivity = 120f;
    private const float GamepadSensitivity = 220f;
    private const float MinPitch = -80f;
    private const float MaxPitch = 80f;
    private const float GamepadDeadzone = 0.2f;

    // Movimiento
    [SerializeField] private float WalkSpeed = 4f;
    [SerializeField] private float SprintSpeed = 6.5f;
    [SerializeField] private float JumpHeight = 1.6f;
    [SerializeField] private float Gravity = -12f;

    private CharacterController _characterController;
    private PlayerInputs _inputActions;
    private RotationAxes _axes = RotationAxes.MouseXAndY;

    // Estado interno
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _pitch;
    private float _verticalSpeed;
    private bool _isSprinting;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        if (_playerCamera == null)
            _playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _inputActions = new PlayerInputs();
    }

    private void OnEnable()
    {
        _inputActions.Enable();

        _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _inputActions.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

        _inputActions.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _inputActions.Player.Look.canceled += ctx => _lookInput = Vector2.zero;

        _inputActions.Player.Jump.performed += ctx => Jump();
        _inputActions.Player.Sprint.performed += ctx => _isSprinting = ctx.ReadValue<float>() > 0.5f;
        _inputActions.Player.Sprint.canceled += ctx => _isSprinting = false;
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        if (_playerCamera == null) return;

        Vector2 look = _lookInput;

        if (UsingGamepad() && look.magnitude < GamepadDeadzone)
            look = Vector2.zero;

        if (look.sqrMagnitude < 0.0001f) return;

        float sensitivity = UsingGamepad() ? GamepadSensitivity : MouseSensitivity;

        float yawDelta = 0f;
        float pitchDelta = 0f;

        if (_axes == RotationAxes.MouseX || _axes == RotationAxes.MouseXAndY)
            yawDelta = look.x * sensitivity * Time.deltaTime;
        if (_axes == RotationAxes.MouseY || _axes == RotationAxes.MouseXAndY)
            pitchDelta = -look.y * sensitivity * Time.deltaTime;

        transform.Rotate(0f, yawDelta, 0f, Space.World);
        _pitch = Mathf.Clamp(_pitch + pitchDelta, MinPitch, MaxPitch);
        _playerCamera.transform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector3 inputDir = new Vector3(_moveInput.x, 0f, _moveInput.y);
        Vector3 moveWorld = (transform.right * inputDir.x + transform.forward * inputDir.z).normalized;

        float speed = (_isSprinting && _characterController.isGrounded) ? SprintSpeed : WalkSpeed;

        if (_characterController.isGrounded && _verticalSpeed < 0f)
            _verticalSpeed = -2f;

        _verticalSpeed += Gravity * Time.deltaTime;

        Vector3 motion = moveWorld * speed + Vector3.up * _verticalSpeed;
        _characterController.Move(motion * Time.deltaTime);
    }

    private void Jump()
    {
        if (!_characterController.isGrounded) return;
        _verticalSpeed = Mathf.Sqrt(JumpHeight * -2f * Gravity);
    }

    private bool UsingGamepad()
    {
        return Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;
    }
}
