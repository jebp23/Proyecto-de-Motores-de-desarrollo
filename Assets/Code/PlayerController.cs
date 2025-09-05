using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 6.5f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Cinemachine")]
    [SerializeField] private Transform cameraTransform;

    [Header("Animation & Sound")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float walkFootstepCadence = 0.5f;
    [SerializeField] private float sprintFootstepCadence = 0.3f;
    [SerializeField] private float noisePerWalkStep = 0.05f;
    [SerializeField] private float noisePerSprintStep = 0.1f;

    private CharacterController _characterController;
    private PlayerInputs _input;
    private NoiseMeter _noiseMeter;

    private Vector2 _moveInput;
    private Vector3 _velocity;
    private bool _isSprinting;
    private bool _isCrouching;

    private float _originalHeight;
    [SerializeField] private float crouchHeight = 1f;

    private float _footstepTimer;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _input = new PlayerInputs();
        _noiseMeter = FindFirstObjectByType<NoiseMeter>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
        _originalHeight = _characterController.height;
    }

    private void OnEnable()
    {
        _input.Enable();
        _input.Player.Move.performed += OnMove;
        _input.Player.Move.canceled += OnMove;
        _input.Player.Sprint.performed += ctx => _isSprinting = true;
        _input.Player.Sprint.canceled += ctx => _isSprinting = false;
        _input.Player.Crouch.performed += OnCrouch;
    }

    private void OnDisable()
    {
        _input.Disable();
        _input.Player.Move.performed -= OnMove;
        _input.Player.Move.canceled -= OnMove;
        _input.Player.Sprint.performed -= ctx => _isSprinting = true;
        _input.Player.Sprint.canceled -= ctx => _isSprinting = false;
        _input.Player.Crouch.performed -= OnCrouch;
    }

    private void Update()
    {
        HandleMovement();
        ApplyGravity();

        float targetHeight = _isCrouching ? crouchHeight : _originalHeight;
        _characterController.height = Mathf.Lerp(_characterController.height, targetHeight, Time.deltaTime * 5f);
        _characterController.center = Vector3.up * (_characterController.height / 2f);

        // Control de pisadas y ruido
        if (_characterController.isGrounded && _moveInput.magnitude > 0.1f)
        {
            _footstepTimer -= Time.deltaTime;
            float currentCadence = _isSprinting ? sprintFootstepCadence : walkFootstepCadence;

            if (_isCrouching)
            {
                currentCadence = -1f; // no sonar pasos en crouch
            }

            if (_footstepTimer <= 0 && currentCadence > 0)
            {
                _footstepTimer = currentCadence;
                PlayFootstepAndAddNoise();
            }
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        _isCrouching = !_isCrouching;
        if (_isSprinting && _isCrouching)
        {
            _isSprinting = false;
        }
    }

    private void HandleMovement()
    {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector3 moveDir = cameraForward * _moveInput.y + cameraRight * _moveInput.x;

        // Velocidad actual según estado
        float currentSpeed = walkSpeed;
        if (_isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (_isSprinting)
        {
            currentSpeed = sprintSpeed;
        }

        // Movimiento físico
        if (moveDir.magnitude >= 0.1f)
        {
            _characterController.Move(moveDir * currentSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Cálculo de animSpeed
        float animSpeed = 0f;

        if (_moveInput.magnitude > 0.1f)
        {
            if (_isCrouching)
            {
                animSpeed = _moveInput.magnitude * 0.5f; // crouch walk hasta 0.5
            }
            else if (_isSprinting)
            {
                animSpeed = _moveInput.magnitude * 1f;   // sprint hasta 1
            }
            else
            {
                animSpeed = _moveInput.magnitude * 0.5f; // caminar normal hasta 0.5
            }
        }

        // Parámetros del Animator
        _animator.SetFloat("MoveSpeed", animSpeed);
        _animator.SetBool("IsSprinting", _isSprinting);
        _animator.SetBool("IsCrouching", _isCrouching);
    }

    private void PlayFootstepAndAddNoise()
    {
        if (AudioManager.I != null)
        {
            AudioManager.I.PlayFootstep(_isSprinting);
        }

        if (_noiseMeter != null)
        {
            float noiseAmount = _isSprinting ? noisePerSprintStep : noisePerWalkStep;
            _noiseMeter.AddNormalizedNoise(noiseAmount);
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
