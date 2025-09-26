using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMonster : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform target;
    [SerializeField] string playerTag = "Player";

    [Header("Movement")]
    [SerializeField] float stoppingDistance = 2f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float idleSpeed = 1.5f;
    [SerializeField] float chaseSpeed = 3.5f;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string walkBool = "isWalking";

    [Header("Detection")]
    [SerializeField] MonoBehaviour detectionStrategyComponent;
    IDetectionStrategy detection;

    [Header("Boards passthrough")]
    [SerializeField] bool ignoreBoardsAtRuntime = false;
    [SerializeField] string boardsRootTag = "Boards";

    [Header("Sanity Damage")]
    [SerializeField] float sanityDamagePerSecond = 0f;

    [Header("Patrol")]
    [SerializeField] bool patrolEnabled = false;
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] float patrolSpeed = 2.5f;
    [SerializeField] float patrolWaitSeconds = 0.5f;
    [SerializeField] bool patrolPingPong = false;
    [SerializeField] bool patrolRandom = false;

    [Header("Strategy Guard")]
    [SerializeField] bool forceEnableStrategyOnStart = true;
    [SerializeField] int enableGuardFrames = 20;

    NavMeshAgent agent;
    bool isChasing;
    Vector3 lastPerceivedTargetPos;
    int patrolIndex;
    int patrolDir = 1;
    float patrolWaitTimer;
    int guardCounter;

    public bool CurrentlyDetecting { get; private set; }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();
        if (!target)
        {
            var p = GameObject.FindWithTag(playerTag);
            if (p) target = p.transform;
        }
        BindDetection();
        if (ignoreBoardsAtRuntime) IgnoreBoardsCollisions();
        if (agent) agent.speed = idleSpeed;
        guardCounter = enableGuardFrames;
    }

    void OnEnable()
    {
        if (forceEnableStrategyOnStart) EnableStrategy();
        guardCounter = enableGuardFrames;
    }

    void Start()
    {
        if (forceEnableStrategyOnStart) EnableStrategy();
    }

    void Update()
    {
        if (forceEnableStrategyOnStart && guardCounter > 0)
        {
            EnableStrategy();
            guardCounter--;
        }

        if (!target || agent == null || detection == null) { PatrolUpdate(false); return; }
        if (!agent.isOnNavMesh) { PatrolUpdate(false); return; }

        CurrentlyDetecting = detection.Detect(target, out var perceivedPos);

        if (CurrentlyDetecting)
        {
            isChasing = true;
            lastPerceivedTargetPos = perceivedPos;
            Chase(perceivedPos);
        }
        else
        {
            if (isChasing)
            {
                float dist = Vector3.Distance(transform.position, lastPerceivedTargetPos);
                if (dist > stoppingDistance * 1.1f) Chase(lastPerceivedTargetPos);
                else { isChasing = false; PatrolUpdate(true); }
            }
            else
            {
                PatrolUpdate(false);
            }
        }
    }

    void PatrolUpdate(bool justLostTarget)
    {
        if (!patrolEnabled || patrolPoints == null || patrolPoints.Length == 0)
        {
            Stop();
            return;
        }

        if (justLostTarget) patrolWaitTimer = patrolWaitSeconds;

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.deltaTime;
            Stop();
            return;
        }

        if (agent.isOnNavMesh)
        {
            agent.speed = patrolSpeed;
            Transform wp = patrolPoints[Mathf.Clamp(patrolIndex, 0, patrolPoints.Length - 1)];
            if (wp)
            {
                if (!agent.pathPending)
                {
                    float d = Vector3.Distance(transform.position, wp.position);
                    if (d <= Mathf.Max(stoppingDistance, agent.stoppingDistance) + 0.1f)
                    {
                        NextPatrolIndex();
                        patrolWaitTimer = patrolWaitSeconds;
                        return;
                    }
                }
                agent.SetDestination(wp.position);
            }
        }

        if (animator && !string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, true);
    }

    void NextPatrolIndex()
    {
        if (patrolRandom)
        {
            if (patrolPoints.Length > 1)
            {
                int next;
                do { next = Random.Range(0, patrolPoints.Length); } while (next == patrolIndex);
                patrolIndex = next;
            }
            return;
        }

        if (!patrolPingPong)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
        else
        {
            patrolIndex += patrolDir;
            if (patrolIndex >= patrolPoints.Length) { patrolIndex = patrolPoints.Length - 2; patrolDir = -1; }
            else if (patrolIndex < 0) { patrolIndex = 1; patrolDir = 1; }
        }
    }

    void Chase(Vector3 pos)
    {
        if (agent.isOnNavMesh)
        {
            agent.speed = chaseSpeed;
            agent.SetDestination(pos);
        }
        if (animator && !string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, true);
        float distToTarget = Vector3.Distance(transform.position, pos);
        if (distToTarget <= stoppingDistance) RotateTowards(pos);
    }

    void Stop()
    {
        if (agent.isOnNavMesh) agent.SetDestination(transform.position);
        if (animator && !string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, false);
    }

    void RotateTowards(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * rotationSpeed);
    }

    void OnTriggerStay(Collider other)
    {
        if (sanityDamagePerSecond <= 0f) return;
        if (!CurrentlyDetecting) return;
        if (!other.CompareTag("Player")) return;
        var s = other.GetComponent<SanitySystem>();
        if (s != null) s.TakeDamage(sanityDamagePerSecond * Time.deltaTime);
    }

    void BindDetection()
    {
        detection = null;
        if (detectionStrategyComponent is IDetectionStrategy ds)
        {
            detection = ds;
            EnableStrategy();
            detection.Initialize(this);
            return;
        }
        var comps = GetComponents<MonoBehaviour>();
        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] is IDetectionStrategy s)
            {
                detection = s;
                detectionStrategyComponent = (MonoBehaviour)s;
                EnableStrategy();
                detection.Initialize(this);
                return;
            }
        }
        var children = GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] is IDetectionStrategy s2)
            {
                detection = s2;
                detectionStrategyComponent = (MonoBehaviour)s2;
                EnableStrategy();
                detection.Initialize(this);
                return;
            }
        }
    }

    void EnableStrategy()
    {
        var beh = detectionStrategyComponent as Behaviour;
        if (beh && !beh.enabled) beh.enabled = true;
    }

    void IgnoreBoardsCollisions()
    {
        var boardsRoot = GameObject.FindWithTag(boardsRootTag);
        if (!boardsRoot) return;
        var myCols = GetComponentsInChildren<Collider>(true);
        var boardCols = boardsRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < myCols.Length; i++)
        {
            var a = myCols[i];
            if (!a) continue;
            for (int j = 0; j < boardCols.Length; j++)
            {
                var b = boardCols[j];
                if (!b) continue;
                Physics.IgnoreCollision(a, b, true);
            }
        }
    }

    public Transform Target => target;
    public NavMeshAgent Agent => agent;
}
