using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class DocumentInteraction : MonoBehaviour
{
    [SerializeField] PlayerRigidBodyController player;
    [SerializeField] Behaviour flashlightBehaviour;

    [SerializeField] GameObject openPrompt;
    [SerializeField] GameObject closePrompt;
    [SerializeField] GameObject documentPanel;
    [SerializeField] GameObject docReadingPanel;
    [SerializeField] TMP_Text documentTextUI;
    [SerializeField] float toggleCooldown = 0.12f;

    [SerializeField] Key interactKey = Key.E;
    [SerializeField] GamepadButton interactButton = GamepadButton.North;
    [SerializeField] Key closeKey = Key.Escape;
    [SerializeField] GamepadButton closeButton = GamepadButton.B;

    readonly HashSet<Document> inRangeDocs = new HashSet<Document>();
    Document currentDocument;
    bool isReading;
    float lastToggleTime;

    void OnEnable()
    {
        if (!player) player = FindFirstObjectByType<PlayerRigidBodyController>();
        if (openPrompt) openPrompt.SetActive(false);
        if (closePrompt) closePrompt.SetActive(false);
        if (documentPanel) documentPanel.SetActive(false);
        if (docReadingPanel) docReadingPanel.SetActive(false);
    }

    void Update()
    {
        if (Time.unscaledTime - lastToggleTime < toggleCooldown) return;

        bool interactPressed =
            (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current[interactButton].wasPressedThisFrame);

        bool closePressed =
            (Keyboard.current != null && Keyboard.current[closeKey].wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current[closeButton].wasPressedThisFrame);

        if (!isReading && interactPressed) TryOpen();
        else if (isReading && (closePressed || interactPressed)) CloseDocument();
    }

    void OnTriggerEnter(Collider other)
    {
        var doc = FindDocument(other);
        if (!doc) return;
        inRangeDocs.Add(doc);
        PickClosest();
        if (!isReading && currentDocument && openPrompt) openPrompt.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        var doc = FindDocument(other);
        if (!doc) return;
        inRangeDocs.Remove(doc);
        if (currentDocument == doc) currentDocument = null;
        PickClosest();
        if (!isReading && openPrompt) openPrompt.SetActive(currentDocument != null);
    }

    void TryOpen()
    {
        if (!currentDocument) return;

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
        if (!c) return null;
        var d = c.GetComponent<Document>();
        if (d) return d;
        d = c.GetComponentInParent<Document>();
        if (d) return d;
        var t = c.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            var cd = t.GetChild(i).GetComponent<Document>();
            if (cd) return cd;
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
            if (!d) continue;
            float dist = Vector3.SqrMagnitude(d.transform.position - p);
            if (dist < bestDist) { bestDist = dist; best = d; }
        }
        currentDocument = best;
    }
}
