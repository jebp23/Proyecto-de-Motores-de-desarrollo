using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundDetectionStrategy : MonoBehaviour, IDetectionStrategy
{
    [SerializeField] float noiseThresholdOn = 0.15f;
    [SerializeField] float noiseThresholdOff = 0.10f;
    [SerializeField] float memorySeconds = 1.5f;

    SphereCollider sphere;
    float currentNoise01;
    bool playerInRange;
    Transform cachedTarget;
    float lastHeardTime;
    Vector3 lastHeardPos;

    public void Initialize(EnemyMonster o)
    {
        sphere = GetComponent<SphereCollider>();
        if (sphere == null) sphere = gameObject.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        enabled = true;
    }

    void OnEnable() { GameEvents.OnNoiseChanged += OnNoiseChanged; }
    void OnDisable() { GameEvents.OnNoiseChanged -= OnNoiseChanged; }

    void OnNoiseChanged(float n) { currentNoise01 = n; }

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

        bool inRange = playerInRange || IsInsideSphere(target);
        bool loud;
        if (currentNoise01 >= noiseThresholdOn) loud = true;
        else if (currentNoise01 <= noiseThresholdOff) loud = false;
        else loud = (Time.time - lastHeardTime) <= memorySeconds;

        bool detected = inRange && loud;

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

    bool IsInsideSphere(Transform t)
    {
        if (sphere == null || t == null) return false;
        float r = sphere.radius * Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
        return Vector3.Distance(transform.position + sphere.center, t.position) <= r;
    }

    static bool sub;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void BootOnce()
    {
        EnableAll();
        if (!sub) { SceneManager.sceneLoaded += OnSceneLoaded; sub = true; }
    }

    static void OnSceneLoaded(Scene s, LoadSceneMode m) { EnableAll(); }

    static void EnableAll()
    {
        var arr = Object.FindObjectsOfType<SoundDetectionStrategy>(true);
        for (int i = 0; i < arr.Length; i++)
        {
            if (!arr[i].enabled) arr[i].enabled = true;
        }
    }
}
