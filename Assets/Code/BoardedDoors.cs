using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardedDoor : MonoBehaviour
{
    [SerializeField] GameObject[] piecesToDisable;
    [SerializeField] Collider triggerZone;
    [SerializeField] GameObject openPrompt;
    [SerializeField] GameObject toolNeededPrompt;
    [SerializeField] float toolNeededSeconds = 1.5f;
    [SerializeField] InputActionReference interactAction;

    bool inRange;
    bool opened;
    Coroutine toolNeededCo;

    void OnEnable()
    {
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
        if (opened) return;
        if (!other.CompareTag("Player")) return;
        inRange = true;

        if (NotesQuestManager.I != null && NotesQuestManager.I.HasTool)
        {
            if (openPrompt) openPrompt.SetActive(true);
        }
        else
        {
            if (toolNeededCo != null) StopCoroutine(toolNeededCo);
            toolNeededCo = StartCoroutine(ShowToolNeeded());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        inRange = false;
        if (openPrompt) openPrompt.SetActive(false);
    }

    void OnInteract(InputAction.CallbackContext _)
    {
        if (!inRange || opened) return;
        if (NotesQuestManager.I == null) return;

        if (NotesQuestManager.I.HasTool)
        {
            Open();
        }
        else
        {
            if (toolNeededCo != null) StopCoroutine(toolNeededCo);
            toolNeededCo = StartCoroutine(ShowToolNeeded());
        }
    }

    IEnumerator ShowToolNeeded()
    {
        if (toolNeededPrompt) toolNeededPrompt.SetActive(true);
        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, toolNeededSeconds));
        if (toolNeededPrompt) toolNeededPrompt.SetActive(false);
    }

    void Open()
    {
        opened = true;
        if (openPrompt) openPrompt.SetActive(false);
        if (toolNeededPrompt) toolNeededPrompt.SetActive(false);
        if (piecesToDisable != null) for (int i = 0; i < piecesToDisable.Length; i++) if (piecesToDisable[i]) piecesToDisable[i].SetActive(false);
        if (triggerZone) triggerZone.enabled = false;
    }
}
