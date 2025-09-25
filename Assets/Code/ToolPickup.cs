using UnityEngine;

public class ToolPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NoteSequencer.I?.SetHasTool(true);
        ToolNotificationUI.I?.Show(6f); 
        Destroy(gameObject);
    }

}
