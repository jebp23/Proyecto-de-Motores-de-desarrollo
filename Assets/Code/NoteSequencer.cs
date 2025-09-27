using UnityEngine;

public class NoteSequencer : MonoBehaviour
{
    public static NoteSequencer I { get; private set; }

    [SerializeField] private NoteContentList content;
    [SerializeField, Min(0)] private int requiredNotesForTool = 4;
    [SerializeField] private GameObject toolPrefab;
    [SerializeField] private string toolSpawnPointName = "ToolSpawnPoint";
    [SerializeField] private bool spawnToolWhenComplete = true;

    public bool HasTool { get; private set; }

    int _assignedCount;
    bool _toolSpawned;
    Transform _toolSpawnPoint;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        var sp = GameObject.Find(toolSpawnPointName);
        _toolSpawnPoint = sp ? sp.transform : null;
    }

    public void EnsureAssignment(Document doc)
    {
        if (doc == null || content == null || content.notes.Count == 0) return;

        if (doc.assignedIndex == -1)
        {
            int idx = Mathf.Clamp(_assignedCount, 0, content.notes.Count - 1);
            doc.assignedIndex = idx;
            doc.documentText = content.notes[idx];
            _assignedCount++;

            if (spawnToolWhenComplete && !_toolSpawned && requiredNotesForTool > 0 && _assignedCount >= requiredNotesForTool)
                SpawnTool();
        }
        else
        {
            int idx = Mathf.Clamp(doc.assignedIndex, 0, content.notes.Count - 1);
            doc.documentText = content.notes[idx];
        }
    }

    void SpawnTool()
    {
        if (_toolSpawned) return;
        if (toolPrefab == null || _toolSpawnPoint == null) return;
        Instantiate(toolPrefab, _toolSpawnPoint.position, _toolSpawnPoint.rotation);
        _toolSpawned = true;
    }

    public void ForceSpawnTool()
    {
        SpawnTool();
    }

    public void SwitchContent(NoteContentList newContent, int requiredForTool, bool spawnToolAtEnd)
    {
        content = newContent;
        requiredNotesForTool = Mathf.Max(0, requiredForTool);
        spawnToolWhenComplete = spawnToolAtEnd;
        _assignedCount = 0;
        _toolSpawned = false;
    }

    public void SetHasTool(bool has) => HasTool = has;
}
