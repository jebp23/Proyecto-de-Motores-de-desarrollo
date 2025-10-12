using System.Collections.Generic;
using UnityEngine;

public class NotesQuestManager : MonoBehaviour
{
    public static NotesQuestManager I { get; private set; }

    [SerializeField] List<NoteContentList> phases = new List<NoteContentList>();
    [SerializeField] List<int> notesRequiredPerPhase = new List<int>();
    [SerializeField] List<bool> spawnToolAtEnd = new List<bool>();
    [SerializeField] GameObject exitTriggerObject;

    int currentPhase;
    int collectedInPhase;
    bool allComplete;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    void Start()
    {
        ApplyPhase(currentPhase);
        if (exitTriggerObject) exitTriggerObject.SetActive(false);
    }

    public void OnNoteCollected()
    {
        if (allComplete) return;
        collectedInPhase = Mathf.Clamp(collectedInPhase + 1, 0, 999);

        int need = GetRequired(currentPhase);
        if (need > 0 && collectedInPhase >= need)
        {
            if (spawnToolAtEnd != null && currentPhase < spawnToolAtEnd.Count && spawnToolAtEnd[currentPhase])
                NoteSequencer.I?.ForceSpawnTool();

            if (currentPhase + 1 < phases.Count)
            {
                currentPhase++;
                ApplyPhase(currentPhase);
            }
            else
            {
                allComplete = true;
                if (exitTriggerObject) exitTriggerObject.SetActive(true);
            }
        }
    }

    void ApplyPhase(int phase)
    {
        collectedInPhase = 0;
        var list = (phase < phases.Count) ? phases[phase] : null;
        int req = GetRequired(phase);
        bool spawnTool = (spawnToolAtEnd != null && phase < spawnToolAtEnd.Count) ? spawnToolAtEnd[phase] : false;
        NoteSequencer.I?.SwitchContent(list, req, spawnTool);
    }

    int GetRequired(int phase)
    {
        if (notesRequiredPerPhase == null || phase >= notesRequiredPerPhase.Count) return 0;
        return Mathf.Max(0, notesRequiredPerPhase[phase]);
    }

    public bool HasTool => NoteSequencer.I != null && NoteSequencer.I.HasTool;
    public bool AllPhasesComplete => allComplete;

    public void SetHasTool(bool on)
    {
        NoteSequencer.I?.SetHasTool(on);
    }
}
