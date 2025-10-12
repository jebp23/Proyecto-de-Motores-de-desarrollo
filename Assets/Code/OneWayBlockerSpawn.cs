using UnityEngine;
using UnityEngine.AI;

public class OneWayBlockerSpawn : MonoBehaviour
{
    public enum Mode { InstantiatePrefab, EnableExisting }

    [SerializeField] string playerTag = "Player";
    [SerializeField] Mode mode = Mode.InstantiatePrefab;

    [SerializeField] GameObject blockerPrefab;
    [SerializeField] GameObject existingBlocker;
    [SerializeField] Transform spawnPoint;

    [SerializeField] bool requireHasTool = true;
    [SerializeField] bool addNavMeshObstacleCarve = true;
    [SerializeField] bool disableTriggerAfterSpawn = true;

    bool spawned;

    void OnTriggerEnter(Collider other)
    {
        if (spawned) return;
        if (!other.CompareTag(playerTag)) return;
        if (requireHasTool && (NotesQuestManager.I == null || !NotesQuestManager.I.HasTool)) return;

        if (mode == Mode.InstantiatePrefab)
        {
            if (!blockerPrefab) return;
            Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
            Quaternion rot = spawnPoint ? spawnPoint.rotation : transform.rotation;
            var go = Instantiate(blockerPrefab, pos, rot);
            if (!go.activeSelf) go.SetActive(true);
            EnsureCarve(go);
        }
        else
        {
            if (!existingBlocker) return;
            if (spawnPoint)
            {
                existingBlocker.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            }
            EnsureCarve(existingBlocker);
            if (!existingBlocker.activeSelf) existingBlocker.SetActive(true);
        }

        spawned = true;
        if (disableTriggerAfterSpawn)
        {
            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
        }
    }

    void EnsureCarve(GameObject go)
    {
        if (!addNavMeshObstacleCarve || !go) return;
        var obst = go.GetComponent<NavMeshObstacle>();
        if (!obst) obst = go.AddComponent<NavMeshObstacle>();
        obst.carving = true;
        if (obst.shape == NavMeshObstacleShape.Box && obst.size == Vector3.zero) obst.size = Vector3.one;
    }
}
