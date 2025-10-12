using UnityEngine;
using UnityEngine.InputSystem;

public class PanelUIMode : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] string uiMap = "UI";
    [SerializeField] string playerMap = "Player";
    [SerializeField] CursorLock cursorLock;

    void OnEnable()
    {
        if (!playerInput) playerInput = FindFirstObjectByType<PlayerInput>();
        if (cursorLock) cursorLock.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (playerInput && !string.IsNullOrEmpty(uiMap))
        {
            var map = playerInput.actions != null ? playerInput.actions.FindActionMap(uiMap, false) : null;
            if (map != null) map.Enable();
            if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != uiMap) playerInput.SwitchCurrentActionMap(uiMap);
        }
    }

    void OnDisable()
    {
        if (playerInput && !string.IsNullOrEmpty(playerMap))
        {
            var map = playerInput.actions != null ? playerInput.actions.FindActionMap(playerMap, false) : null;
            if (map != null) map.Enable();
            if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != playerMap) playerInput.SwitchCurrentActionMap(playerMap);
        }
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (cursorLock) cursorLock.enabled = true;
    }
}
