using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DocumentInteraction : MonoBehaviour
{
    [SerializeField] string debugId = "DocInt";
    [SerializeField] PlayerRigidBodyController player;
    [SerializeField] Behaviour flashlightBehaviour;
    [SerializeField] GameObject openPrompt;
    [SerializeField] GameObject closePrompt;
    [SerializeField] GameObject documentPanel;
    [SerializeField] GameObject docReadingPanel;
    [SerializeField] TMP_Text documentTextUI;
    [SerializeField] float toggleCooldown = 0.12f;
    [SerializeField] InputActionReference interactActionRef;
    [SerializeField] PlayerInput playerInput;

    readonly HashSet<Document> inRangeDocs = new HashSet<Document>();
    Document currentDocument;
    bool isReading;
    float lastToggleTime;
    InputAction interactAction;

    void OnEnable()
    {
        if (!player) player = FindFirstObjectByType<PlayerRigidBodyController>();
        if (!playerInput) playerInput = FindFirstObjectByType<PlayerInput>();
        if (openPrompt) openPrompt.SetActive(false);
        if (closePrompt) closePrompt.SetActive(false);
        if (documentPanel) documentPanel.SetActive(false);
        if (docReadingPanel) docReadingPanel.SetActive(false);

        interactAction = interactActionRef != null ? interactActionRef.action : null;
        if (interactAction == null && playerInput != null && playerInput.actions != null)
            interactAction = playerInput.actions.FindAction("Player/Interact", false) ?? playerInput.actions.FindAction("Interact", false);

        if (interactAction != null)
        {
            if (interactAction.actionMap != null && !interactAction.actionMap.enabled) interactAction.actionMap.Enable();
            interactAction.started += OnInteract;
            interactAction.performed += OnInteract;
            if (!interactAction.enabled) interactAction.Enable();
        }
    }

    void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.started -= OnInteract;
            interactAction.performed -= OnInteract;
            interactAction.Disable();
        }
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (Time.unscaledTime - lastToggleTime < toggleCooldown) return;
        if (!isReading) TryOpen(); else CloseDocument();
    }

    void OnTriggerEnter(Collider other)
    {
        var doc = FindDocument(other);
        if (doc == null) return;
        inRangeDocs.Add(doc);
        PickClosest();
        if (!isReading && currentDocument != null && openPrompt) openPrompt.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        var doc = FindDocument(other);
        if (doc == null) return;
        inRangeDocs.Remove(doc);
        if (currentDocument == doc) currentDocument = null;
        PickClosest();
        if (!isReading && openPrompt) openPrompt.SetActive(currentDocument != null);
    }

    void TryOpen()
    {
        if (currentDocument == null) return;
        NoteSequencer.I?.EnsureAssignment(currentDocument);
        if (documentTextUI) documentTextUI.text = currentDocument.documentText;
        if (openPrompt) openPrompt.SetActive(false);
        if (documentPanel) documentPanel.SetActive(true);
        if (docReadingPanel) docReadingPanel.SetActive(true);
        if (closePrompt) closePrompt.SetActive(true);
        if (player) player.SetInputEnabled(false);
        if (flashlightBehaviour) flashlightBehaviour.enabled = false;
        Time.timeScale = 0f;
        if (!currentDocument.collected)
        {
            currentDocument.collected = true;
            GameManager.I?.DocumentCollected(currentDocument);
        }
        isReading = true;
        lastToggleTime = Time.unscaledTime;
    }

    void CloseDocument()
    {
        if (documentPanel) documentPanel.SetActive(false);
        if (docReadingPanel) docReadingPanel.SetActive(false);
        if (closePrompt) closePrompt.SetActive(false);
        Time.timeScale = 1f;
        if (player) player.SetInputEnabled(true);
        if (flashlightBehaviour) flashlightBehaviour.enabled = true;
        isReading = false;
        lastToggleTime = Time.unscaledTime;
        PickClosest();
        if (openPrompt) openPrompt.SetActive(currentDocument != null);
    }

    Document FindDocument(Collider c)
    {
        if (c == null) return null;
        var d = c.GetComponent<Document>();
        if (d != null) return d;
        d = c.GetComponentInParent<Document>();
        if (d != null) return d;
        var t = c.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            var cd = t.GetChild(i).GetComponent<Document>();
            if (cd != null) return cd;
        }
        return null;
    }

    void PickClosest()
    {
        Document best = null;
        float bestDist = float.MaxValue;
        Vector3 p = transform.position;
        foreach (var d in inRangeDocs)
        {
            if (d == null) continue;
            float dist = Vector3.SqrMagnitude(d.transform.position - p);
            if (dist < bestDist) { bestDist = dist; best = d; }
        }
        currentDocument = best;
    }
}
