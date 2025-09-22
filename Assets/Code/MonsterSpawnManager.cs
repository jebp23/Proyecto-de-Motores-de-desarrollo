using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class MonsterSpawnManager : MonoBehaviour
{
    
    [SerializeField] private List<Transform> spawnPoints = new();
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private string monsterTag = "Monster";
    [SerializeField] private bool spawnOnSceneLoaded = true;

    private GameObject currentMonster;

    private void Awake()
    {    
        currentMonster = GameObject.FindWithTag(monsterTag);
    }

    private void OnEnable()
    {
        if (spawnOnSceneLoaded)
            SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.OnLevelRestart += OnLevelRestart;

    }

    private void OnDisable()
    {
        if (spawnOnSceneLoaded)
            SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEvents.OnLevelRestart -= OnLevelRestart;
    }

    private void Start()
    {
        if (!spawnOnSceneLoaded)
            PlaceMonster();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {       
        currentMonster = GameObject.FindWithTag(monsterTag);
        PlaceMonster();
    }

    private void OnLevelRestart()                     
    {
        if (currentMonster == null)
            currentMonster = GameObject.FindWithTag(monsterTag);

        PlaceMonster();
    }

    public void PlaceMonster()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("[MonsterSpawnManager] No spawn points available.");
            return;
        }

        Transform p = spawnPoints[Random.Range(0, spawnPoints.Count)];
        SpawnOrMove(p.position, p.rotation);
    }

    private void SpawnOrMove(Vector3 pos, Quaternion rot)
    {
          if (currentMonster == null)
        {
            if (monsterPrefab == null)
            {
                Debug.LogWarning("[MonsterSpawnManager] No prefab available");
                return;
            }
            currentMonster = Instantiate(monsterPrefab, pos, rot);
            return;
        }


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
    }
}
