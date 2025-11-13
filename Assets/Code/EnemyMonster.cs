using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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

    [Header("SFX Sources (3D)")]
    [SerializeField] private AudioSource detectionSource;
    [SerializeField] private AudioSource stunSource;
    [SerializeField, Range(0f, 1f)] private float detectionVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float stunVolume = 1f;

    [Header("Patrol")]
    [SerializeField] bool patrolEnabled = false;
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] float patrolSpeed = 2.5f;
    [SerializeField] float patrolWaitSeconds = 0.5f;
    [SerializeField] bool patrolPingPong = false;
    [SerializeField] bool patrolRandom = false;

    NavMeshAgent agent;
    IDetectionStrategy detection;
    bool isChasing;
    bool isStunned;
    float stunEndTime;
    bool detectionArmed = true;
    float lastNotDetectTime;
    const float detectionRearmDelay = 1.0f;
    public bool CurrentlyDetecting { get; private set; }
    bool prevDetect;
    int patrolIndex;
    int patrolDir = 1;
    float patrolWaitTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();
        var player = GameObject.FindWithTag(playerTag);
        if (player) target = player.transform;
        BindDetection();
        if (agent)
        {
            agent.speed = idleSpeed;
            agent.stoppingDistance = stoppingDistance;
        }
    }

    void Update()
    {
        if (isStunned)
        {
            if (Time.time >= stunEndTime) EndStun();
            else return;
        }

        if (!target || detection == null || agent == null) return;
        prevDetect = CurrentlyDetecting;
        Vector3 pos;
        CurrentlyDetecting = detection.Detect(target, out pos);

        if (CurrentlyDetecting && !prevDetect && detectionArmed && detectionSource)
        {
            detectionSource.PlayOneShot(detectionSource.clip, detectionVolume);
            detectionArmed = false;
        }

        if (!CurrentlyDetecting)
        {
            if (Time.time - lastNotDetectTime >= detectionRearmDelay)
                detectionArmed = true;
            lastNotDetectTime = Time.time;
        }

        if (CurrentlyDetecting)
        {
            isChasing = true;
            Chase(pos);
        }
        else
        {
            if (isChasing)
            {
                float d = Vector3.Distance(transform.position, pos);
                if (d > stoppingDistance * 1.1f) Chase(pos);
                else isChasing = false;
            }
            PatrolUpdate();
        }
    }

    void Chase(Vector3 pos)
    {
        if (!agent.isOnNavMesh) return;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(pos);
        if (animator && !string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, true);
    }

    void HoldPosition()
    {
        if (!agent.isOnNavMesh) return;
        agent.ResetPath();
        agent.isStopped = true;
        if (animator && !string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, false);
    }

    void PatrolUpdate()
    {
        if (!patrolEnabled || patrolPoints == null || patrolPoints.Length == 0)
        {
            HoldPosition();
            return;
        }

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.deltaTime;
            HoldPosition();
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
                agent.isStopped = false;
                agent.SetDestination(wp.position);
            }
        }

        if (animator && !string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, true);
    }

    void NextPatrolIndex()
    {
        if (!patrolPingPong)
        {
            if (patrolRandom && patrolPoints.Length > 1)
            {
                int next;
                do { next = Random.Range(0, patrolPoints.Length); } while (next == patrolIndex);
                patrolIndex = next;
            }
            else patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            return;
        }

        patrolIndex += patrolDir;
        if (patrolIndex >= patrolPoints.Length) { patrolIndex = patrolPoints.Length - 2; patrolDir = -1; }
        else if (patrolIndex < 0) { patrolIndex = 1; patrolDir = 1; }
    }

    public void ApplyLightStun(float duration)
    {
        if (isStunned) return;
        isStunned = true;
        stunEndTime = Time.time + duration;
        agent.ResetPath();
        agent.isStopped = true;
        if (animator && !string.IsNullOrEmpty(stunnedBool)) animator.SetBool(stunnedBool, true);
        if (stunSource) stunSource.PlayOneShot(stunSource.clip, stunVolume);
    }

    void EndStun()
    {
        isStunned = false;
        if (agent) agent.isStopped = false;
        if (animator && !string.IsNullOrEmpty(stunnedBool)) animator.SetBool(stunnedBool, false);
    }

    void BindDetection()
    {
        if (detectionStrategyComponent is IDetectionStrategy ds)
        {
            detection = ds;
            detection.Initialize(this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (sanityDamagePerSecond <= 0f) return;
        if (!other.CompareTag(playerTag)) return;
        if (!CurrentlyDetecting) return;

        var s = other.GetComponent<SanitySystem>();
        if (s != null)
            s.TakeDamage(sanityDamagePerSecond * Time.deltaTime);
    }


    public void SuppressFor(float seconds)
    {
        StartCoroutine(SuppressCoroutine(seconds));
    }

    IEnumerator SuppressCoroutine(float seconds)
    {
        bool prevChasing = isChasing;
        bool prevDetecting = CurrentlyDetecting;
        isChasing = false;
        CurrentlyDetecting = false;
        if (agent)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
        yield return new WaitForSecondsRealtime(seconds);
        if (agent) agent.isStopped = false;
        isChasing = prevChasing;
        CurrentlyDetecting = prevDetecting;
    }

    public void UsePatrolRoute(PatrolRoute route)
    {
        if (route == null) return;
        patrolPoints = route.Points;
        patrolEnabled = patrolPoints != null && patrolPoints.Length > 0;
        patrolIndex = 0;
    }

    public void WarpAwayFrom(Vector3 origin, float minDistance)
    {
        if (agent == null) return;
        Vector3 dir = transform.position - origin;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f)
            dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        dir.Normalize();
        Vector3 targetPos = origin + dir * Mathf.Max(0.1f, minDistance);
        if (NavMesh.SamplePosition(targetPos, out var hit, minDistance + 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);
        else
            agent.Warp(targetPos);
    }

    public void ResetStateAfterRespawn(Vector3 playerPosition, float minDistance)
    {
        isChasing = false;
        CurrentlyDetecting = false;
        isStunned = false;
        detectionArmed = true;
        lastNotDetectTime = Time.time;
        if (agent)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        float dist = Vector3.Distance(transform.position, playerPosition);
        if (dist < minDistance)
            WarpAwayFrom(playerPosition, minDistance);

        if (animator && !string.IsNullOrEmpty(walkBool))
            animator.SetBool(walkBool, false);
    }


    public void ReactivateAI()
    {
        isChasing = false;
        CurrentlyDetecting = false;
        isStunned = false;
        detectionArmed = true;

        if (agent)
        {
            agent.ResetPath();
            agent.isStopped = false;
            agent.speed = idleSpeed; 
        }
    }
}
