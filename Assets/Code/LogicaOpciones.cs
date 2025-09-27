using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LogicaOpciones : MonoBehaviour
{
    public bool isPaused = false;
    public ControladorOpciones panelOpciones;
    EventSystem eventSystem;
    bool allowPause = true;

    void Start()
    {
        panelOpciones = GameObject.FindGameObjectWithTag("opciones").GetComponent<ControladorOpciones>();
        eventSystem = EventSystem.current;
        isPaused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!allowPause) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    public void TogglePause()
    {
        if (!allowPause) return;
        if (GameManager.I != null && GameManager.I.CurrentState != GameState.Playing) return;
        if (isPaused) EsconderOpciones(); else MostrarOpciones();
    }

    public void MostrarOpciones()
    {
        if (!allowPause) return;
        if (GameManager.I != null && GameManager.I.CurrentState != GameState.Playing) return;
        if (panelOpciones && panelOpciones.optionsScreen) panelOpciones.optionsScreen.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        var c = FindFirstObjectByType<CursorLock>(FindObjectsInactive.Include);
        if (c) c.OnPauseChanged(true);
    }

    public void EsconderOpciones()
    {
        if (panelOpciones && panelOpciones.optionsScreen) panelOpciones.optionsScreen.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        var c = FindFirstObjectByType<CursorLock>(FindObjectsInactive.Include);
        if (c) c.OnPauseChanged(false);
    }

    public void BlockPause(bool block)
    {
        allowPause = !block;
        if (block && isPaused) EsconderOpciones();
    }

    public void OnClickRestartLevel()
    {
        var pause = FindFirstObjectByType<LogicaOpciones>(FindObjectsInactive.Include);
        if (pause) pause.EsconderOpciones();
        GameManager.I?.RestartLevel();
    }

    public void OnClickResume()
    {
        FindFirstObjectByType<LogicaOpciones>(FindObjectsInactive.Include)?.EsconderOpciones();
    }
}
