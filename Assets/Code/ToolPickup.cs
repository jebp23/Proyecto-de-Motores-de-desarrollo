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
        if (voiceClip != null) PersistentAudioManager.I?.PlayVoice(voiceClip);
        NotesQuestManager.I?.SetHasTool(true);
        NoteSequencer.I?.SetHasTool(true);
        ToolNotificationUI.I?.Show();
        gameObject.SetActive(false);
    }
}
