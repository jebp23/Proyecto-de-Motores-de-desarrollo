using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerRigidBodyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private NoiseMeter noiseMeter;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 4.0f;
    [SerializeField] private float crouchSpeed = 1.2f;
    [SerializeField] private float rotationSpeed = 540f;
    [SerializeField] private float inputDeadzone = 0.12f;

    [Header("Animation & Sound")]
    [SerializeField] private string paramMoveSpeed = "MoveSpeed";
    [SerializeField] private string paramIsSprinting = "IsSprinting";
    [SerializeField] private string paramIsCrouching = "IsCrouching";
    [SerializeField] private float walkFootstepCadence = 0.5f;
    [SerializeField] private float sprintFootstepCadence = 0.3f;

    [Header("Noise & Sound")]
    [SerializeField] private float noisePerWalkStep = 0.05f;
    [SerializeField] private float noisePerSprintStep = 0.1f;

    [Header("Misc")]
    [SerializeField] private float smallVelocityEpsilon = 0.05f;

    // --- Inputs ---
    private PlayerInputs playerInputs;
    private Vector2 moveInput;
    private bool isSprinting;
    private bool isCrouching;
    private float _footstepTimer;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (noiseMeter == null) Debug.LogError("NoiseMeter reference not set on PlayerRigidBodyController.");

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        playerInputs = new PlayerInputs();
    }

    private void OnEnable()
    {
        playerInputs.Enable();
        playerInputs.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInputs.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        playerInputs.Player.Sprint.performed += ctx => isSprinting = true;
        playerInputs.Player.Sprint.canceled += ctx => isSprinting = false;

        playerInputs.Player.Crouch.performed += ctx => isCrouching = !isCrouching;
    }

    private void OnDisable()
    {
        playerInputs.Player.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInputs.Player.Move.canceled -= ctx => moveInput = Vector2.zero;

        playerInputs.Player.Sprint.performed -= ctx => isSprinting = true;
        playerInputs.Player.Sprint.canceled -= ctx => isSprinting = false;

        playerInputs.Player.Crouch.performed -= ctx => isCrouching = !isCrouching;
        playerInputs.Disable();
    }

    private void FixedUpdate()
    {
        UpdateMovement();
        UpdateAnimations();
    }

    private void Update()
    {
        HandleFootsteps();
    }

    private void UpdateMovement()
    {
        Vector3 desiredDir = GetDesiredDirection();
        float targetSpeed = GetTargetSpeed();

        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 desiredPlanarVelocity = desiredDir * targetSpeed;

        rb.linearVelocity = new Vector3(desiredPlanarVelocity.x, currentVelocity.y, desiredPlanarVelocity.z);

        if (desiredDir.sqrMagnitude > smallVelocityEpsilon)
        {
            Quaternion targetRot = Quaternion.LookRotation(desiredDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private Vector3 GetDesiredDirection()
    {
        if (moveInput.magnitude < inputDeadzone) return Vector3.zero;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        return forward * moveInput.y + right * moveInput.x;
    }

    private float GetTargetSpeed()
    {
        if (isCrouching) return crouchSpeed;
        if (isSprinting) return runSpeed;
        return walkSpeed;
    }

    private void UpdateAnimations()
    {
        Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float animSpeed = planarVelocity.magnitude;

        float mappedSpeed = 0f;
        if (animSpeed > smallVelocityEpsilon)
        {
            if (isCrouching) mappedSpeed = Mathf.Clamp01(animSpeed / crouchSpeed) * 0.5f;
            else if (isSprinting) mappedSpeed = Mathf.Clamp01(animSpeed / runSpeed);
            else mappedSpeed = Mathf.Clamp01(animSpeed / walkSpeed) * 0.5f;
        }

        animator.SetFloat(paramMoveSpeed, mappedSpeed);
        animator.SetBool(paramIsSprinting, isSprinting);
        animator.SetBool(paramIsCrouching, isCrouching);
    }

    private void HandleFootsteps()
    {
        Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float currentSpeed = planarVelocity.magnitude;
        float cadence = isSprinting ? sprintFootstepCadence : walkFootstepCadence;

        if (currentSpeed > smallVelocityEpsilon)
        {
            _footstepTimer += Time.deltaTime;
            if (_footstepTimer >= cadence)
            {
                PlayFootstepAndAddNoise();
                _footstepTimer = 0f;
            }
        }
        else
        {
            _footstepTimer = 0f;
        }
    }

    private void PlayFootstepAndAddNoise()
    {
        // Si el personaje está agachado, no hace ruido y no reproduce sonido de pasos.
        if (isCrouching)
        {
            // Opcional: Podrías hacer un ruido muy bajo si quisieras
            // noiseMeter?.AddNormalizedNoise(0.001f);
            return;
        }

        if (AudioManager.I != null)
        {
            AudioManager.I.PlayFootstep(isSprinting);
        }

        if (noiseMeter != null)
        {
            float noiseAmount = isSprinting ? noisePerSprintStep : noisePerWalkStep;
            noiseMeter.AddNormalizedNoise(noiseAmount);
        }
    }
}