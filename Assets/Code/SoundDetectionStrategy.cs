using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundDetectionStrategy : MonoBehaviour, IDetectionStrategy
{
    [SerializeField] float noiseThresholdOn = 0.15f;
    [SerializeField] float noiseThresholdOff = 0.10f;
    [SerializeField] float memorySeconds = 1.5f;

    Collider volume;         
    float currentNoise01;
    bool playerInRange;
    Transform cachedTarget;
    float lastHeardTime;
    Vector3 lastHeardPos;

    public void Initialize(EnemyMonster o)
    {      
        volume = GetComponent<Collider>();
        if (volume == null)
        {          
            volume = gameObject.AddComponent<BoxCollider>();
        }
        volume.isTrigger = true; 
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
        bool inRange = playerInRange || IsInsideVolume(target);

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

    bool IsInsideVolume(Transform t)
    {
        if (volume == null || t == null) return false;
        Vector3 p = t.position;
        Vector3 cp = volume.ClosestPoint(p);    
        return (cp - p).sqrMagnitude <= 1e-6f;
    }
}
