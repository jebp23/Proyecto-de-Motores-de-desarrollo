using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLock : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] bool autoLockOnStart = true;
    [SerializeField] Key toggleKey = Key.Escape;   // libera/bloquea con Esc

    bool shouldBeLocked;

    void Start()
    {
        if (autoLockOnStart) LockCursor();
    }

    void Update()
    {
        // Permite alternar (útil para abrir menús sin sistema de pausa aún)
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked) UnlockCursor();
            else LockCursor();
        }
    }

    // Si el editor o el SO quitan el foco, al volver lo recapturamos.
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && shouldBeLocked) LockCursor();
    }

    public void LockCursor()
    {
        shouldBeLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        shouldBeLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Útil si tenés un GameManager con pausa:
    public void OnPauseChanged(bool paused)
    {
        if (paused) UnlockCursor(); else LockCursor();
    }
}
