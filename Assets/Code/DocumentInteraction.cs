using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DocumentInteraction : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private FlashlightController flashlightController;
    [SerializeField] private GameObject openPrompt;
    [SerializeField] private GameObject closePrompt;
    [SerializeField] private GameObject documentPanel;
    [SerializeField] private TMP_Text documentTextUI;
    [SerializeField] private GameObject docReadingPanel;

    private InputAction interactAction;
    private Document currentDocument;
    private bool isReading;

    private void Awake()
    {
        if (!playerInput) playerInput = FindObjectOfType<PlayerInput>();
        if (!playerController) playerController = FindObjectOfType<PlayerController>();
        if (!flashlightController) flashlightController = FindObjectOfType<FlashlightController>();

        if (openPrompt) openPrompt.SetActive(false);
        if (closePrompt) closePrompt.SetActive(false);
        if (documentPanel) documentPanel.SetActive(false);
        if (docReadingPanel) docReadingPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            // Suscribirse a la acción "Interact" que siempre está activa
            interactAction = playerInput.actions.FindAction("Interact");
            if (interactAction != null)
            {
                interactAction.started += OnInteract;
                interactAction.Enable();
            }
            else
            {
                Debug.LogError("[DocumentInteraction] No se encontró la acción 'Interact'.");
            }
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.started -= OnInteract;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var doc = other.GetComponent<Document>();
        if (doc)
        {
            currentDocument = doc;
            if (openPrompt) openPrompt.SetActive(true);
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

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (isReading)
        {
            // Cerrar documento
            if (documentPanel) documentPanel.SetActive(false);
            if (closePrompt) closePrompt.SetActive(false);
            if (docReadingPanel) docReadingPanel.SetActive(false);

            Time.timeScale = 1f;
            if (playerController) playerController.enabled = true;
            if (flashlightController) flashlightController.enabled = true;

            // Lógica para resetear el estado del prompt
            if (currentDocument == null)
            {
                openPrompt.SetActive(false);
            }
            else
            {
                openPrompt.SetActive(true);
            }

            isReading = false;
        }
        else
        {
            // Abrir documento
            if (currentDocument != null)
            {
                if (documentTextUI) documentTextUI.text = currentDocument.documentText;
                if (documentPanel) documentPanel.SetActive(true);
                if (docReadingPanel) docReadingPanel.SetActive(true);
                if (openPrompt) openPrompt.SetActive(false);
                if (closePrompt) closePrompt.SetActive(true);

                Time.timeScale = 0f;
                if (playerController) playerController.enabled = false;
                if (flashlightController) flashlightController.enabled = false;

                currentDocument.collected = true;
                isReading = true;
            }
            else
            {
                Debug.LogWarning("[DocumentInteraction] No hay documento en rango para abrir.");
            }
        }
    }
}