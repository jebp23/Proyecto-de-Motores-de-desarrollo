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
    [SerializeField] private CapsuleCollider capsule; 

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 4.0f;
    [SerializeField] private float crouchSpeed = 1.2f;
    [SerializeField] private float rotationSpeed = 540f;
    [SerializeField] private float inputDeadzone = 0.12f;

    [Header("Animation & Sound")]
    private string paramMoveSpeed = "MoveSpeed";
    private string paramIsSprinting = "IsSprinting";
    private string paramIsCrouching = "IsCrouching";
    [SerializeField] private float walkFootstepCadence = 0.5f;
    [SerializeField] private float sprintFootstepCadence = 0.3f;

    [Header("Noise & Sound")]
    [SerializeField] private float noisePerWalkStep = 0.05f;
    [SerializeField] private float noisePerSprintStep = 0.1f;

    [Header("Misc")]
    [SerializeField] private float smallVelocityEpsilon = 0.05f;

    [Header("Crouch Collider")]
    [SerializeField] private float crouchHeight = 1.2f;
    [SerializeField] private Vector3 crouchCenter = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private float colliderLerpSpeed = 12f;
    [SerializeField] private LayerMask standUpMask = ~0;
    [SerializeField] private float standUpSkin = 0.02f;

    [Header("Footstep Volumes")]
    [SerializeField, Range(0f, 1f)] float walkStepVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] float runStepVolume = 1f;
    [SerializeField] AudioSource footstepSource;


    private float originalHeight;
    private Vector3 originalCenter;
    private bool wasCrouching;


    private PlayerInputs playerInputs;
    private Vector2 moveInput;
    private bool isSprinting;
    private bool isCrouching;
    private float _footstepTimer;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (capsule == null) capsule = GetComponent<CapsuleCollider>();
        if (noiseMeter == null) Debug.LogError("NoiseMeter reference not set on PlayerRigidBodyController.");

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        originalHeight = capsule.height;
        originalCenter = capsule.center;
        wasCrouching = false;

        playerInputs = new PlayerInputs();
    }

    private void OnEnable()
    {
        playerInputs.Enable();
      
        playerInputs.Player.Move.performed += OnMovePerformed;
        playerInputs.Player.Move.canceled += OnMoveCanceled;

        playerInputs.Player.Sprint.performed += OnSprintPerformed;
        playerInputs.Player.Sprint.canceled += OnSprintCanceled;

        playerInputs.Player.Crouch.performed += OnCrouchPerformed;
    }

    private void OnDisable()
    {
        playerInputs.Player.Move.performed -= OnMovePerformed;
        playerInputs.Player.Move.canceled -= OnMoveCanceled;

        playerInputs.Player.Sprint.performed -= OnSprintPerformed;
        playerInputs.Player.Sprint.canceled -= OnSprintCanceled;

        playerInputs.Player.Crouch.performed -= OnCrouchPerformed;

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
        HandleCrouchCollider(); 
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => moveInput = Vector2.zero;

    private void OnSprintPerformed(InputAction.CallbackContext ctx) => isSprinting = true;
    private void OnSprintCanceled(InputAction.CallbackContext ctx) => isSprinting = false;

    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        if (isCrouching)
        {
            if (CanStandUp())
                isCrouching = false;
            else
                isCrouching = true; 
        }
        else
        {
            isCrouching = true;
        }
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
        if (isCrouching) return;

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

    private void HandleCrouchCollider()
    {
        float targetHeight = isCrouching ? crouchHeight : originalHeight;
        Vector3 targetCenter = isCrouching ? crouchCenter : originalCenter;

        capsule.height = Mathf.Lerp(capsule.height, targetHeight, Time.deltaTime * colliderLerpSpeed);
        capsule.center = Vector3.Lerp(capsule.center, targetCenter, Time.deltaTime * colliderLerpSpeed);
    }

    
    private bool CanStandUp()
    {
        float targetHeight = originalHeight;
        float radius = Mathf.Max(capsule.radius - standUpSkin, 0.01f);

        Vector3 worldCenter = transform.TransformPoint(originalCenter);
        float halfHeight = Mathf.Max(targetHeight * 0.5f - radius, 0f);

        Vector3 bottom = worldCenter + Vector3.down * halfHeight;
        Vector3 top = worldCenter + Vector3.up * halfHeight;

        Collider[] hits = Physics.OverlapCapsule(
            bottom, top, radius, standUpMask, QueryTriggerInteraction.Ignore
        );

        foreach (var col in hits)
        {
            if (col.transform.IsChildOf(transform)) continue;
            return false;
        }
        return true;
    }

    public void SetInputEnabled(bool on)
    {
        if (on) playerInputs.Enable();
        else playerInputs.Disable();
    }

}
