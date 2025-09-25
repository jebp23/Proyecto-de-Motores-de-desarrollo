using UnityEngine;

public class NoteSequencer : MonoBehaviour
{
    public static NoteSequencer I { get; private set; }

    [Header("Data")]
    [SerializeField] private NoteContentList content;
    [SerializeField, Min(1)] private int requiredNotesForTool = 4;

    [Header("Tool Spawn")]
    [SerializeField] private GameObject toolPrefab;
    [SerializeField] private string toolSpawnPointName = "ToolSpawnPoint";

    public bool HasTool { get; private set; }

    private int _assignedCount = 0;
    private bool _toolSpawned = false;
    private Transform _toolSpawnPoint;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        var sp = GameObject.Find(toolSpawnPointName);
        _toolSpawnPoint = sp ? sp.transform : null;
        if (_toolSpawnPoint == null)
            Debug.LogWarning($"[NoteSequencer] No se encontró '{toolSpawnPointName}' en la escena.");
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

            // ¿Debemos spawnear la herramienta?
            if (!_toolSpawned && _assignedCount >= requiredNotesForTool)
            {
                SpawnTool();
            }
        }
        else
        {
            // Ya tenía asignación previa: solo asegura que el texto siga correcto por si se editó el SO
            int idx = Mathf.Clamp(doc.assignedIndex, 0, content.notes.Count - 1);
            doc.documentText = content.notes[idx];
        }
    }

    private void SpawnTool()
    {
        if (_toolSpawned) return;
        if (toolPrefab == null || _toolSpawnPoint == null)
        {
            Debug.LogWarning("[NoteSequencer] No puedo spawnear tool (falta prefab o ToolSpawnPoint).");
            return;
        }
        Instantiate(toolPrefab, _toolSpawnPoint.position, _toolSpawnPoint.rotation);
        _toolSpawned = true;
    }

    public void SetHasTool(bool has) => HasTool = has;
}
