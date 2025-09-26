using UnityEngine;

public class VisionDetectionStrategy : MonoBehaviour, IDetectionStrategy
{
    [Header("Vision")]
    [SerializeField, Min(0f)] private float detectionRange = 10f;
    [SerializeField, Range(0f, 360f)] private float fieldOfViewAngle = 120f;
    [SerializeField] private float eyeHeight = 1.6f;

    [Header("Line of Sight")]
    [SerializeField] private LayerMask occludersMask = ~0; 
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private float memorySeconds = 1.5f; 

    private EnemyMonster owner;
    private float lastSeenTime;
    private Vector3 lastSeenPos;

    public void Initialize(EnemyMonster owner)
    {
        this.owner = owner;
    }

    public bool Detect(Transform target, out Vector3 targetPos)
    {
        targetPos = Vector3.zero;
        if (!owner || !target) return false;

        Vector3 from = owner.transform.position + Vector3.up * eyeHeight;
        Vector3 to = target.position + Vector3.up * eyeHeight;

        float dist = Vector3.Distance(from, to);
        if (dist > detectionRange)
        {
            if ((Time.time - lastSeenTime) <= memorySeconds)
            {
                targetPos = lastSeenPos;
            }
            return false;
        }

        Vector3 dir = (to - from).normalized;
        float angle = Vector3.Angle(owner.transform.forward, dir);
        if (angle > fieldOfViewAngle * 0.5f)
        {
            if ((Time.time - lastSeenTime) <= memorySeconds)
                targetPos = lastSeenPos;
            return false;
        }

        if (requireLineOfSight)
        {
            if (Physics.Linecast(from, to, out RaycastHit hit, occludersMask, QueryTriggerInteraction.Ignore))
            {
                if (!hit.transform.IsChildOf(target))
                {
                    if ((Time.time - lastSeenTime) <= memorySeconds)
                        targetPos = lastSeenPos;
                    return false;
                }
            }
        }

        lastSeenPos = target.position;
        lastSeenTime = Time.time;
        targetPos = lastSeenPos;
        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (fieldOfViewAngle < 360f)
        {
            Vector3 forward = transform.forward;
            Quaternion left = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0);
            Quaternion right = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, left * forward * detectionRange);
            Gizmos.DrawRay(transform.position, right * forward * detectionRange);
        }
    }
#endif
}
