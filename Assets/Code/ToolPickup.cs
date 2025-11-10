using UnityEngine;

public class ToolPickup : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] AudioSource pickupSource;
    [SerializeField] AudioClip voiceClip;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (pickupSource != null) pickupSource.Play();
        AudioManager.I?.PlayVO_ToolFound();
        NotesQuestManager.I?.SetHasTool(true);
        NoteSequencer.I?.SetHasTool(true);
        ToolNotificationUI.I?.Show();
        gameObject.SetActive(false);
    }
}
