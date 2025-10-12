using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightBurstAttack : MonoBehaviour
{
    [SerializeField] Light flashlight;
    [SerializeField] float burstRange = 15f;
    [SerializeField] LayerMask monsterMask = ~0;
    [SerializeField] float stunDuration = 2.5f;
    [SerializeField] float cooldown = 5f;
    [SerializeField] float vfxBoostIntensity = 3f;
    [SerializeField] float vfxBoostAngle = 10f;
    [SerializeField] float vfxBoostTime = 0.2f;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] string actionName = "LightAttack";

    float nextReadyTime;
    InputAction action;
    float baseIntensity;
    float baseSpotAngle;
    bool vfxBusy;

    void Awake()
    {
        if (!flashlight) flashlight = GetComponentInChildren<Light>();
        if (!playerInput) playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput) action = playerInput.actions.FindAction(actionName);
        baseIntensity = flashlight ? flashlight.intensity : 0f;
        baseSpotAngle = flashlight ? flashlight.spotAngle : 0f;
    }

    void OnEnable()
    {
        if (action != null) action.performed += OnAttackPerformed;
    }

    void OnDisable()
    {
        if (action != null) action.performed -= OnAttackPerformed;
    }

    void Update()
    {
        if (action == null)
        {
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) TryAttack();
            else if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) TryAttack();
        }
    }

    void OnAttackPerformed(InputAction.CallbackContext ctx) { TryAttack(); }

    void TryAttack()
    {
        if (Time.time < nextReadyTime) return;
        if (!flashlight || !flashlight.enabled) return;

        if (Physics.Raycast(flashlight.transform.position, flashlight.transform.forward, out RaycastHit hit, burstRange, monsterMask, QueryTriggerInteraction.Ignore))
        {
            var enemy = hit.collider.GetComponentInParent<EnemyMonster>();
            if (enemy && enemy.gameObject.activeInHierarchy)
            {
                enemy.ApplyLightStun(stunDuration);
            }
        }

        if (!vfxBusy && flashlight) StartCoroutine(BurstVFX());
        nextReadyTime = Time.time + cooldown;
    }

    IEnumerator BurstVFX()
    {
        vfxBusy = true;
        float t = 0f;
        float i0 = baseIntensity;
        float a0 = baseSpotAngle;
        flashlight.intensity = i0 + vfxBoostIntensity;
        flashlight.spotAngle = a0 - vfxBoostAngle;
        while (t < vfxBoostTime)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        flashlight.intensity = i0;
        flashlight.spotAngle = a0;
        vfxBusy = false;
    }

    public float CooldownRemaining => Mathf.Max(0f, nextReadyTime - Time.time);
    public float CooldownDuration => cooldown;
}
