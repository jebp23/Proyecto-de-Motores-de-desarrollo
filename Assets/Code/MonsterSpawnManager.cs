using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class MonsterSpawnManager : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] GameObject monsterPrefab;
    [SerializeField] string monsterTag = "Monster";
    [SerializeField] bool spawnOnSceneLoaded = true;
    [SerializeField] bool randomizeSpawn = true;

    [Header("Patrol")]
    [SerializeField] PatrolRoute patrolRoute;

    [Header("Respawn If Missing")]
    [SerializeField] bool respawnIfDestroyed = true;
    [SerializeField] float checkInterval = 2f;

    GameObject currentMonster;
    float nextCheck;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (spawnOnSceneLoaded) Spawn();
    }

    void Update()
    {
        if (!respawnIfDestroyed) return;
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + checkInterval;

        if (currentMonster == null || !currentMonster.activeInHierarchy)
            Spawn();
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        if (spawnOnSceneLoaded) Spawn();
    }

    public void Spawn()
    {
        if (!monsterPrefab) return;
        if (spawnPoints == null || spawnPoints.Count == 0) return;

        Transform p = randomizeSpawn
            ? spawnPoints[Random.Range(0, spawnPoints.Count)]
            : spawnPoints[0];

        Vector3 pos = p.position;
        Quaternion rot = p.rotation;

        if (currentMonster == null)
        {
            currentMonster = Instantiate(monsterPrefab, pos, rot);
        }
        else
        {
            var agent = currentMonster.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                agent.Warp(pos);
                currentMonster.transform.rotation = rot;
            }
            else
            {
                currentMonster.transform.SetPositionAndRotation(pos, rot);
            }
            currentMonster.SetActive(true);
        }

        var em = currentMonster.GetComponent<EnemyMonster>();
        if (em != null && patrolRoute != null)
            em.UsePatrolRoute(patrolRoute);
    }
}
