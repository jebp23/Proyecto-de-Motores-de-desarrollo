using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class VictoryUI : MonoBehaviour
{
    [SerializeField] GameObject root;
    [SerializeField] string mainMenuSceneName = "MainMenu";
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
            if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != uiMap)
                playerInput.SwitchCurrentActionMap(uiMap);
        }

        if (root) root.SetActive(true);
        Time.timeScale = 0f;
    }

    void OnDisable()
    {
        if (playerInput && !string.IsNullOrEmpty(playerMap))
        {
            var map = playerInput.actions != null ? playerInput.actions.FindActionMap(playerMap, false) : null;
            if (map != null) map.Enable();
            if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != playerMap)
                playerInput.SwitchCurrentActionMap(playerMap);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (cursorLock) cursorLock.enabled = true;
    }

    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        GameManager.I?.RestartLevel();
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
