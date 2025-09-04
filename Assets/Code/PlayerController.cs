using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 6.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Cinemachine")]
    [SerializeField] private Transform cameraTransform; // Referencia a la transform de la cámara principal

    private CharacterController _characterController;
    private PlayerInputs _input;

    private Vector2 _moveInput;
    private Vector3 _velocity;
    private bool _isSprinting;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _input = new PlayerInputs();

        // Asignar la referencia a la cámara principal
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        _input.Enable();
        _input.Player.Move.performed += OnMove;
        _input.Player.Move.canceled += OnMove;
        _input.Player.Sprint.performed += ctx => _isSprinting = true;
        _input.Player.Sprint.canceled += ctx => _isSprinting = false;
    }

    private void OnDisable()
    {
        _input.Player.Move.performed -= OnMove;
        _input.Player.Move.canceled -= OnMove;
        _input.Disable();
    }

    private void Update()
    {
        HandleMovement();
        ApplyGravity();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void HandleMovement()
    {
        // Obtener la dirección de la cámara sin la componente vertical (y)
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector3 moveDir = cameraForward * _moveInput.y + cameraRight * _moveInput.x;
        float currentSpeed = _isSprinting ? sprintSpeed : walkSpeed;

        if (moveDir.magnitude >= 0.1f)
        {
            // Movimiento
            _characterController.Move(moveDir * currentSpeed * Time.deltaTime);

            // Rotación suave hacia la dirección de movimiento
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }
}