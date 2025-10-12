using UnityEngine;
using UnityEngine.InputSystem;

public class ExitGoalTrigger : MonoBehaviour
{
    [SerializeField] GameObject escapePrompt;
    [SerializeField] InputActionReference interactAction;

    bool inRange;

    void OnEnable()
    {
        if (escapePrompt) escapePrompt.SetActive(false);
        if (interactAction && interactAction.action != null)
        {
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (interactAction && interactAction.action != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!NotesQuestManager.I || !NotesQuestManager.I.AllPhasesComplete) return;
        inRange = true;
        if (escapePrompt) escapePrompt.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        inRange = false;
        if (escapePrompt) escapePrompt.SetActive(false);
    }

    void OnInteract(InputAction.CallbackContext _)
    {
        if (!inRange) return;
        if (!NotesQuestManager.I || !NotesQuestManager.I.AllPhasesComplete) return;
        if (escapePrompt) escapePrompt.SetActive(false);
        GameManager.I?.TriggerVictory();
    }
}
