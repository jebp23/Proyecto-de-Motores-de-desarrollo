using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DocumentInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;                   // to read Interact action
    [SerializeField] private PlayerRigidBodyController player;          // <-- updated: RB controller
    [SerializeField] private FlashlightController flashlightController;
    [SerializeField] private GameObject openPrompt;
    [SerializeField] private GameObject closePrompt;
    [SerializeField] private GameObject documentPanel;
    [SerializeField] private TMP_Text documentTextUI;
    [SerializeField] private GameObject docReadingPanel;

    [Header("UI Toggle")]
    [SerializeField] private float toggleCooldown = 0.15f;             // anti-bounce in unscaled time

    private InputAction interactAction;
    private Document currentDocument;
    private bool isReading;
    private float nextToggleAllowedAt;                                  // unscaled time

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
            else
            {
                Debug.LogError("[DocumentInteraction] 'Interact' action not found.");
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
        if (currentDocument == null)
        {
            Debug.LogWarning("[DocumentInteraction] No document in range to open.");
            return;
        }

        if (documentTextUI) documentTextUI.text = currentDocument.documentText;
        if (documentPanel) documentPanel.SetActive(true);
        if (docReadingPanel) docReadingPanel.SetActive(true);
        if (openPrompt) openPrompt.SetActive(false);
        if (closePrompt) closePrompt.SetActive(true);

        // pause world & enter UI input mode
        Time.timeScale = 0f;
        //if (player) player.EnterUIMode();
        if (flashlightController) flashlightController.enabled = false;

        currentDocument.collected = true;
        isReading = true;
    }

    private void CloseDocument()
    {
        if (!isReading) return;

        if (documentPanel) documentPanel.SetActive(false);
        if (docReadingPanel) docReadingPanel.SetActive(false);
        if (closePrompt) closePrompt.SetActive(false);

        // resume world & exit UI input mode
        //if (player) player.ExitUIMode();
        Time.timeScale = 1f;

        if (flashlightController) flashlightController.enabled = true;

        // restore prompt state if still in trigger
        if (currentDocument && openPrompt) openPrompt.SetActive(true);

        isReading = false;
    }
}
