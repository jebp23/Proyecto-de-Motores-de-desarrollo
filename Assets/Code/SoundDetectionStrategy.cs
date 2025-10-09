// CAMBIOS EN SoundDetectionStrategy.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundDetectionStrategy : MonoBehaviour, IDetectionStrategy
{
    [SerializeField] float noiseThresholdOn = 0.15f;
    [SerializeField] float noiseThresholdOff = 0.10f;
    [SerializeField] float memorySeconds = 1.5f;

    // BEFORE: SphereCollider sphere;
    // AFTER:
    Collider volume;         // BoxCollider, CapsuleCollider o SphereCollider (lo que tenga el GO)
    float currentNoise01;
    bool playerInRange;
    Transform cachedTarget;
    float lastHeardTime;
    Vector3 lastHeardPos;

    public void Initialize(EnemyMonster o)
    {
        // Usar el collider que YA tiene el monstruo
        volume = GetComponent<Collider>();
        if (volume == null)
        {
            // Fallback si no tiene ninguno (mejor Box por pisos)
            volume = gameObject.AddComponent<BoxCollider>();
        }
        volume.isTrigger = true; // aseguramos trigger para OnTriggerEnter/Exit
        enabled = true;
    }

    void OnEnable() { GameEvents.OnNoiseChanged += OnNoiseChanged; }
    void OnDisable() { GameEvents.OnNoiseChanged -= OnNoiseChanged; }

    void OnNoiseChanged(float n) { currentNoise01 = n; } // se alimenta desde NoiseMeter/RaiseNoiseChanged
                                                         // 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    public bool Detect(Transform target, out Vector3 targetPos)
    {
        if (cachedTarget == null) cachedTarget = target;

        // BEFORE: bool inRange = playerInRange || IsInsideSphere(target);
        // AFTER:
        bool inRange = playerInRange || IsInsideVolume(target);

        bool loud;
        if (currentNoise01 >= noiseThresholdOn) loud = true;
        else if (currentNoise01 <= noiseThresholdOff) loud = false;
        else loud = (Time.time - lastHeardTime) <= memorySeconds;

        bool detected = inRange && loud; // mismo criterio de "ruido suficiente" + "en volumen"
                                         // (umbral On/Off + memoria) :contentReference[oaicite:2]{index=2}

        if (detected && cachedTarget != null)
        {
            lastHeardPos = cachedTarget.position;
            lastHeardTime = Time.time;
            targetPos = lastHeardPos;
            return true;
        }

        if ((Time.time - lastHeardTime) <= memorySeconds)
        {
            targetPos = lastHeardPos;
            return false;
        }

        targetPos = Vector3.zero;
        return false;
    }

    // NUEVO en lugar de IsInsideSphere
    bool IsInsideVolume(Transform t)
    {
        if (volume == null || t == null) return false;
        Vector3 p = t.position;
        Vector3 cp = volume.ClosestPoint(p);    // si está adentro, ClosestPoint = punto mismo
        return (cp - p).sqrMagnitude <= 1e-6f;
    }

    // (boot/enable helpers se quedan como estaban)
}
