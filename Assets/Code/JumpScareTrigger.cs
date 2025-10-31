using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JumpScareTrigger : MonoBehaviour
{
    public AudioSource scareAudio;

    public bool oneTimeTrigger = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && oneTimeTrigger) return;

        if (other.CompareTag("Player"))
        {
            if (scareAudio != null)
            {
                scareAudio.Play();
            }

            hasTriggered = true;
        }
    }
}
