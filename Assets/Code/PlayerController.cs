using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float mouseSensitivity = 120f;
    [SerializeField] private float gamepadSensitivity = 220f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float gamepadDeadzone = 0.2f;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 6.5f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Footsteps / Noise (Tuneable)")]
    [SerializeField] private NoiseMeter _noiseMeter;
    [SerializeField] private float walkStepInterval = 0.44f;
    [SerializeField] private float runStepInterval = 0.30f;
    [SerializeField] private float crouchStepInterval = 0.60f;
    [SerializeField, Range(0f, 1f)] private float walkNoiseFactor = 0.33f; // ~1/3
    [SerializeField, Range(0f, 1f)] private float runNoiseFactor = 0.70f;  // ~2/3+

    [Header("Crouch Settings")]
    [SerializeField] private float crouchCameraHeight = 0.5f;
    [SerializeField] private float standCameraHeight = 1.6f;
    [SerializeField] private float crouchLerpSpeed = 8f;

    private CharacterController _characterController;
    private PlayerInputs _input;
    private Vector2 _moveInput, _lookInput;
    private float _pitch, _verticalSpeed;
    private bool _isSprinting, _isCrouching;
    private float _stepTimer;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        if (!_playerCamera) _playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _input = new PlayerInputs();
    }

    void OnEnable()
    {
        _input.Enable();

        _input.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _input.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

        _input.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _input.Player.Look.canceled += ctx => _lookInput = Vector2.zero;

        _input.Player.Sprint.performed += ctx => _isSprinting = ctx.ReadValue<float>() > 0.5f;
        _input.Player.Sprint.canceled += ctx => _isSprinting = false;

        _input.Player.Crouch.performed += ctx => ToggleCrouch();
    }

    void OnDisable()
    {
        _input.Disable();
    }

    void Update()
    {
        HandleLook();
        HandleMove();
        HandleFootsteps();
        HandleCrouchCamera();
    }

    private void HandleLook()
    {
        if (!_playerCamera) return;

        Vector2 li = _lookInput;
        if (UsingGamepad() && li.magnitude < gamepadDeadzone)
            li = Vector2.zero;

        if (li.sqrMagnitude < 0.0001f) return;

        float sens = UsingGamepad() ? gamepadSensitivity : mouseSensitivity;

        float yawDelta = li.x * sens * Time.deltaTime;
        float pitchDelta = -li.y * sens * Time.deltaTime;

        transform.Rotate(0f, yawDelta, 0f, Space.World);

        _pitch = Mathf.Clamp(_pitch + pitchDelta, minPitch, maxPitch);
        _playerCamera.transform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
    }

    private void HandleMove()
    {
        Vector3 inputDir = new Vector3(_moveInput.x, 0f, _moveInput.y);
        Vector3 moveWorld = (transform.right * inputDir.x + transform.forward * inputDir.z).normalized;

        float speed = walkSpeed;
        if (_isCrouching)
            speed = crouchSpeed;
        else if (_isSprinting && _characterController.isGrounded)
            speed = sprintSpeed;

        if (_characterController.isGrounded && _verticalSpeed < 0f)
            _verticalSpeed = -2f;
        _verticalSpeed += gravity * Time.deltaTime;

        Vector3 motion = moveWorld * speed + Vector3.up * _verticalSpeed;
        _characterController.Move(motion * Time.deltaTime);
    }

    private void HandleFootsteps()
    {
        float speedXZ = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z).magnitude;

        if (_characterController.isGrounded && speedXZ > 0.1f)
        {
            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                bool running = _isSprinting;
                bool crouching = _isCrouching;

                if (!crouching && _noiseMeter != null)
                {
                    float amount = running ? runNoiseFactor : walkNoiseFactor;
                    _noiseMeter.AddNormalizedNoise(amount);
                }

                if (!crouching)
                {
                    AudioManager.I?.PlayFootstep(running);
                }
                _stepTimer = crouching ? crouchStepInterval : (running ? runStepInterval : walkStepInterval);
            }
        }
        else
        {
            _stepTimer = 0f;
        }
    }

    private void ToggleCrouch()
    {
        _isCrouching = !_isCrouching;
    }

    private void HandleCrouchCamera()
    {
        if (_playerCamera == null) return;

        Vector3 camPos = _playerCamera.transform.localPosition;
        float targetY = _isCrouching ? crouchCameraHeight : standCameraHeight;

        camPos.y = Mathf.Lerp(camPos.y, targetY, Time.deltaTime * crouchLerpSpeed);
        _playerCamera.transform.localPosition = camPos;
    }

    private bool UsingGamepad() => Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;
}
