using UnityEngine;

public class ToolPickup : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        NotesQuestManager.I?.SetHasTool(true);
        NoteSequencer.I?.SetHasTool(true);
        ToolNotificationUI.I?.Show();
        gameObject.SetActive(false);
    }
}
