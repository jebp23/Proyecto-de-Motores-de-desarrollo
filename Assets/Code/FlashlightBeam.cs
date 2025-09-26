using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightBeam : MonoBehaviour
{
    [SerializeField] Light flashlight;
    [SerializeField] float burstRange = 15f;
    [SerializeField] LayerMask monsterMask = ~0;
    [SerializeField] float stunDuration = 2.5f;
    [SerializeField] float cooldown = 5f;
    [SerializeField] float vfxBoostIntensity = 3f;
    [SerializeField] float vfxBoostAngle = 10f;
    [SerializeField] float vfxBoostTime = 0.2f;
    [SerializeField] bool requireFlashlightOn = true;

    [SerializeField] AudioClip attackSfx;
    [SerializeField, Range(0f, 1f)] float attackSfxVolume = 1f;
    [SerializeField] AudioSource sfxSource;

    [SerializeField] PlayerInput playerInput;
    [SerializeField] string toggleActionName = "Flashlight";
    [SerializeField] string attackActionName = "LightAttack";

    InputAction toggleAction;
    InputAction attackAction;

    float nextReadyTime;
    float baseIntensity;
    float baseSpotAngle;
    bool vfxBusy;

    void Awake()
    {
        if (!flashlight) flashlight = GetComponentInChildren<Light>();
        if (!playerInput) playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput)
        {
            toggleAction = playerInput.actions.FindAction(toggleActionName);
            attackAction = playerInput.actions.FindAction(attackActionName);
        }
        if (flashlight)
        {
            baseIntensity = flashlight.intensity;
            baseSpotAngle = flashlight.spotAngle;
        }
    }

    void OnEnable()
    {
        if (toggleAction != null) toggleAction.performed += OnToggle;
        if (attackAction != null) attackAction.performed += OnAttack;
    }

    void OnDisable()
    {
        if (toggleAction != null) toggleAction.performed -= OnToggle;
        if (attackAction != null) attackAction.performed -= OnAttack;
    }

    void OnToggle(InputAction.CallbackContext ctx)
    {
        if (!flashlight) return;
        flashlight.enabled = !flashlight.enabled;
    }

    void OnAttack(InputAction.CallbackContext ctx)
    {
        TryAttack();
    }

    void TryAttack()
    {
        if (Time.time < nextReadyTime) return;
        if (requireFlashlightOn && (!flashlight || !flashlight.enabled)) return;

        if (flashlight)
        {
            Ray ray = new Ray(flashlight.transform.position, flashlight.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, burstRange, monsterMask, QueryTriggerInteraction.Ignore))
            {
                var enemy = hit.collider.GetComponentInParent<EnemyMonster>();
                if (enemy && enemy.gameObject.activeInHierarchy) enemy.ApplyLightStun(stunDuration);
            }
        }

        if (attackSfx)
        {
            if (sfxSource) sfxSource.PlayOneShot(attackSfx, attackSfxVolume);
            else AudioSource.PlayClipAtPoint(attackSfx, transform.position, attackSfxVolume);
        }

        if (!vfxBusy && flashlight) StartCoroutine(BurstVFX());
        nextReadyTime = Time.time + cooldown;
    }

    IEnumerator BurstVFX()
    {
        vfxBusy = true;
        float i0 = baseIntensity;
        float a0 = baseSpotAngle;
        flashlight.intensity = i0 + vfxBoostIntensity;
        flashlight.spotAngle = a0 - vfxBoostAngle;
        float t = 0f;
        while (t < vfxBoostTime) { t += Time.unscaledDeltaTime; yield return null; }
        flashlight.intensity = i0;
        flashlight.spotAngle = a0;
        vfxBusy = false;
    }

    public float CooldownRemaining => Mathf.Max(0f, nextReadyTime - Time.time);
    public float CooldownDuration => cooldown;
}
