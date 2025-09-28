using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLock : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] bool autoLockOnStart = true;
    [SerializeField] Key toggleKey = Key.Escape;   

    bool shouldBeLocked;

    void Start()
    {
        if (autoLockOnStart) LockCursor();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked) UnlockCursor();
            else LockCursor();
        }
    }

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

    public void OnPauseChanged(bool paused)
    {
        if (paused) UnlockCursor(); else LockCursor();
    }
}
