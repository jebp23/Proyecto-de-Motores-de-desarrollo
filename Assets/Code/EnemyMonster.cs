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
    [SerializeField] string stunnedBool = "isStunned";

    [Header("Detection")]
    [SerializeField] MonoBehaviour detectionStrategyComponent;

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

    [Header("Collision")]
    [SerializeField] bool ignorePlayerBodyCollision = true;

    [Header("Light Stun")]
    [SerializeField] bool canBeStunnedByLight = true;
    [SerializeField] float stunSeconds = 2.5f;
    [SerializeField] bool freezeAgentOnStun = true;
    [SerializeField] AudioClip stunSfx;
    [SerializeField, Range(0f, 1f)] float stunSfxVolume = 1f;
    [SerializeField] AudioSource sfxSource;

    IDetectionStrategy detection;
    NavMeshAgent agent;
    bool isChasing;
    Vector3 lastPerceivedTargetPos;
    int patrolIndex;
    int patrolDir = 1;
    float patrolWaitTimer;
    int guardCounter;
    bool isStunned;
    float stunEndTime;
    float suppressUntilTime;
    GameObject playerGO;

    public bool CurrentlyDetecting { get; private set; }
    public bool IsStunned => isStunned;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();
        playerGO = GameObject.FindWithTag(playerTag);
        if (!target && playerGO) target = playerGO.transform;
        BindDetection();
        if (agent) { agent.speed = idleSpeed; agent.stoppingDistance = stoppingDistance; }
        guardCounter = enableGuardFrames;
        if (ignorePlayerBodyCollision) IgnorePlayerBodyCollisions();
    }

    void OnEnable()
    {
        if (forceEnableStrategyOnStart) EnableStrategy();
        guardCounter = enableGuardFrames;
    }

    void Update()
    {
        if (isStunned)
        {
            if (Time.time >= stunEndTime) EndStun();
            else { Stop(); return; }
        }

        if (Time.time < suppressUntilTime)
        {
            PatrolUpdate(false);
            return;
        }

        if (!target || agent == null || detection == null) { PatrolUpdate(false); return; }
        if (!agent.isOnNavMesh) { PatrolUpdate(false); return; }

        if (forceEnableStrategyOnStart && guardCounter > 0) { EnableStrategy(); guardCounter--; }

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

        if (!patrolPingPong) patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
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
            agent.isStopped = false;
            agent.SetDestination(pos);
        }
        if (animator && !string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, true);
        Vector3 flat = pos; flat.y = transform.position.y;
        Vector3 dir = flat - transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * rotationSpeed);
        }
    }

    void Stop()
    {
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = false;
        }
        if (animator && !string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, false);
    }

    void OnTriggerStay(Collider other)
    {
        if (sanityDamagePerSecond <= 0f) return;
        if (!other.CompareTag(playerTag)) return;
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

    void IgnorePlayerBodyCollisions()
    {
        if (!playerGO) playerGO = GameObject.FindWithTag(playerTag);
        if (!playerGO) return;
        var pcols = playerGO.GetComponentsInChildren<Collider>(true);
        var mycols = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < mycols.Length; i++)
        {
            if (mycols[i] == null || mycols[i].isTrigger) continue;
            for (int j = 0; j < pcols.Length; j++)
            {
                if (pcols[j] == null || pcols[j].isTrigger) continue;
                Physics.IgnoreCollision(mycols[i], pcols[j], true);
            }
        }
    }

    public void ApplyLightStun(float customDuration)
    {
        if (!canBeStunnedByLight) return;
        float d = customDuration > 0f ? customDuration : stunSeconds;
        isStunned = true;
        stunEndTime = Time.time + d;
        if (agent) { agent.ResetPath(); if (freezeAgentOnStun) agent.isStopped = true; }
        CurrentlyDetecting = false;
        isChasing = false;
        if (animator && !string.IsNullOrEmpty(stunnedBool)) animator.SetBool(stunnedBool, true);
        if (stunSfx)
        {
            if (sfxSource) sfxSource.PlayOneShot(stunSfx, stunSfxVolume);
            else AudioSource.PlayClipAtPoint(stunSfx, transform.position, stunSfxVolume);
        }
    }

    void EndStun()
    {
        isStunned = false;
        if (agent) agent.isStopped = false;
        if (animator && !string.IsNullOrEmpty(stunnedBool)) animator.SetBool(stunnedBool, false);
    }

    public void UsePatrolRoute(PatrolRoute route)
    {
        if (route == null) { patrolEnabled = false; patrolPoints = null; return; }
        patrolPoints = route.Points;
        patrolEnabled = patrolPoints != null && patrolPoints.Length > 0;
        patrolIndex = 0;
    }

    public void SetPatrolPoints(Transform[] pts)
    {
        patrolPoints = pts;
        patrolEnabled = patrolPoints != null && patrolPoints.Length > 0;
        patrolIndex = 0;
    }

    public void SuppressFor(float seconds)
    {
        suppressUntilTime = Mathf.Max(suppressUntilTime, Time.time + Mathf.Max(0f, seconds));
        isChasing = false;
        CurrentlyDetecting = false;
        if (agent) { agent.ResetPath(); agent.isStopped = false; }
    }

    public void WarpAwayFrom(Vector3 origin, float minDistance)
    {
        if (agent == null) return;
        Vector3 dir = transform.position - origin;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        dir.Normalize();
        Vector3 target = origin + dir * Mathf.Max(0.1f, minDistance);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, minDistance + 2f, NavMesh.AllAreas)) agent.Warp(hit.position);
        else agent.Warp(target);
        isChasing = false;
        CurrentlyDetecting = false;
        agent.ResetPath();
    }

    public Transform Target => target;
    public NavMeshAgent Agent => agent;
}
