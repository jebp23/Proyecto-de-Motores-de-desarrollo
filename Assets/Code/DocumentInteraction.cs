using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DocumentInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerRigidBodyController player;
    [SerializeField] private FlashlightController flashlightController;
    [SerializeField] private GameObject openPrompt;
    [SerializeField] private GameObject closePrompt;
    [SerializeField] private GameObject documentPanel;
    [SerializeField] private TMP_Text documentTextUI;
    [SerializeField] private GameObject docReadingPanel;
    [SerializeField] private float toggleCooldown = 0.15f;

    private InputAction interactAction;
    private Document currentDocument;
    private bool isReading;
    private float nextToggleAllowedAt;

    private void Awake()
    {
        if (!playerInput) playerInput = FindFirstObjectByType<PlayerInput>();
        if (!player) player = FindFirstObjectByType<PlayerRigidBodyController>();
        if (!flashlightController) flashlightController = FindFirstObjectByType<FlashlightController>();
        if (openPrompt) openPrompt.SetActive(false);
        if (closePrompt) closePrompt.SetActive(false);
        if (documentPanel) documentPanel.SetActive(false);
        if (docReadingPanel) docReadingPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            interactAction = playerInput.actions.FindAction("Interact");
            if (interactAction != null)
            {
                interactAction.started += OnInteractStarted;
                interactAction.Enable();
            }
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.started -= OnInteractStarted;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var doc = other.GetComponent<Document>();
        if (doc)
        {
            currentDocument = doc;
            if (openPrompt && !isReading) openPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var doc = other.GetComponent<Document>();
        if (doc && doc == currentDocument)
        {
            currentDocument = null;
            if (openPrompt) openPrompt.SetActive(false);
        }
    }

    private void OnInteractStarted(InputAction.CallbackContext ctx)
    {
        if (Time.unscaledTime < nextToggleAllowedAt) return;
        if (isReading) CloseDocument();
        else OpenDocument();
        nextToggleAllowedAt = Time.unscaledTime + toggleCooldown;
    }

    private void OpenDocument()
    {
        if (currentDocument == null) return;
        NoteSequencer.I?.EnsureAssignment(currentDocument);
        if (documentTextUI) documentTextUI.text = currentDocument.documentText;
        if (documentPanel) documentPanel.SetActive(true);
        if (docReadingPanel) docReadingPanel.SetActive(true);
        if (openPrompt) openPrompt.SetActive(false);
        if (closePrompt) closePrompt.SetActive(true);
        Time.timeScale = 0f;
        if (flashlightController) flashlightController.enabled = false;
        currentDocument.collected = true;
        GameManager.I?.DocumentCollected(currentDocument);
        isReading = true;
    }

    private void CloseDocument()
    {
        if (!isReading) return;
        if (documentPanel) documentPanel.SetActive(false);
        if (docReadingPanel) docReadingPanel.SetActive(false);
        if (closePrompt) closePrompt.SetActive(false);
        Time.timeScale = 1f;
        if (flashlightController) flashlightController.enabled = true;
        if (currentDocument && openPrompt) openPrompt.SetActive(true);
        isReading = false;
    }
}
