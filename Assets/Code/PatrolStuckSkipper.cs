using UnityEngine;
using UnityEngine.AI;

public class PatrolStuckSkipper : MonoBehaviour
{
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] bool pingPong;
    [SerializeField] bool random;
    [SerializeField] float checkEvery = 0.25f;
    [SerializeField] float stuckSpeedThreshold = 0.05f;
    [SerializeField] float stuckSecondsToSkip = 2f;
    [SerializeField] float arriveDistance = 0.5f;

    NavMeshAgent agent;
    int index;
    int dir = 1;
    float timer;
    Vector3 lastPos;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        lastPos = transform.position;
        if (patrolPoints != null && patrolPoints.Length > 0) SetDestination(ClosestIndex());
        InvokeRepeating(nameof(Tick), checkEvery, checkEvery);
    }

    void Tick()
    {
        if (!agent || patrolPoints == null || patrolPoints.Length == 0) return;
        if (!agent.enabled) return;

        var distMoved = Vector3.Distance(transform.position, lastPos);
        lastPos = transform.position;
        var speed = distMoved / checkEvery;
        var trying = agent.hasPath && agent.remainingDistance > arriveDistance && agent.pathStatus != NavMeshPathStatus.PathInvalid;
        if (trying && speed < stuckSpeedThreshold) timer += checkEvery; else timer = 0f;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial) timer += checkEvery;

        if (timer >= stuckSecondsToSkip) { Next(); timer = 0f; return; }

        if (!agent.hasPath || agent.remainingDistance <= arriveDistance) Next();
    }

    void Next()
    {
        if (random && patrolPoints.Length > 1)
        {
            int next;
            do { next = Random.Range(0, patrolPoints.Length); } while (next == index);
            index = next;
        }
        else if (pingPong)
        {
            if (index == 0) dir = 1;
            else if (index == patrolPoints.Length - 1) dir = -1;
            index += dir;
        }
        else
        {
            index = (index + 1) % patrolPoints.Length;
        }
        SetDestination(index);
    }

    void SetDestination(int i)
    {
        if (i < 0 || i >= patrolPoints.Length) return;
        var t = patrolPoints[i];
        if (!t) return;
        NavMeshHit hit;
        var pos = t.position;
        if (NavMesh.SamplePosition(pos, out hit, 1.0f, agent.areaMask)) pos = hit.position;
        agent.SetDestination(pos);
    }

    int ClosestIndex()
    {
        var best = 0;
        var bestD = float.MaxValue;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (!patrolPoints[i]) continue;
            var d = Vector3.SqrMagnitude(transform.position - patrolPoints[i].position);
            if (d < bestD) { bestD = d; best = i; }
        }
        return best;
    }
}
