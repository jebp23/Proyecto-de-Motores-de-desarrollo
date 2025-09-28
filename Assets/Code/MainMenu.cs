using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject background;
    [SerializeField] GameObject gameTitle;
    [SerializeField] GameObject btnPlay;
    [SerializeField] GameObject btnHowToPlay;
    [SerializeField] GameObject btnOptions;
    [SerializeField] GameObject btnExit;

    [SerializeField] GameObject optionsCanvasRoot;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject howToPlayPanel;

    [SerializeField] Canvas mainMenuCanvas;
    [SerializeField] Canvas optionsCanvas;
    [SerializeField] bool bringOptionsToFront = true;

    [SerializeField] PlayerInput playerInput;
    [SerializeField] string uiMap = "UI";
    [SerializeField] string playerMap = "Player";
    [SerializeField] Behaviour cursorLockBehaviour;

    void Awake()
    {
        if (!mainMenuCanvas) mainMenuCanvas = GetComponentInParent<Canvas>();
        if (!optionsCanvas && optionsCanvasRoot) optionsCanvas = optionsCanvasRoot.GetComponentInChildren<Canvas>(true);
        if (!optionsPanel && optionsCanvasRoot) optionsPanel = FindDeep(optionsCanvasRoot.transform, "OptionsMenu")?.gameObject;

        SafeSetActive(optionsPanel, false);
        SafeSetActive(optionsCanvasRoot, false);
        SafeSetActive(howToPlayPanel, false);

        if (bringOptionsToFront && optionsCanvas)
        {
            optionsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            optionsCanvas.overrideSorting = true;
            int baseOrder = mainMenuCanvas ? mainMenuCanvas.sortingOrder : 0;
            optionsCanvas.sortingOrder = baseOrder + 10;
        }

        ApplyMenuInputState(true);
        ShowMainMenu(true);
    }

    void OnEnable()
    {
        ApplyMenuInputState(true);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) ApplyMenuInputState(true);
    }

    public void StartLevel(string levelName)
    {
        ApplyMenuInputState(false);
        SceneManager.LoadScene(levelName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenOptions()
    {
        SafeSetActive(howToPlayPanel, false);
        SafeSetActive(background, true);
        SafeSetActive(gameTitle, true);
        SafeSetActive(btnPlay, false);
        SafeSetActive(btnHowToPlay, false);
        SafeSetActive(btnOptions, false);
        SafeSetActive(btnExit, false);
        SafeSetActive(optionsCanvasRoot, true);
        SafeSetActive(optionsPanel, true);
    }

    public void CloseOptions()
    {
        SafeSetActive(optionsPanel, false);
        SafeSetActive(optionsCanvasRoot, false);
        ShowMainMenu(true);
    }

    public void OpenHowToPlay()
    {
        SafeSetActive(optionsPanel, false);
        SafeSetActive(optionsCanvasRoot, false);
        SafeSetActive(background, true);
        SafeSetActive(gameTitle, true);
        SafeSetActive(btnPlay, false);
        SafeSetActive(btnHowToPlay, false);
        SafeSetActive(btnOptions, false);
        SafeSetActive(btnExit, false);
        SafeSetActive(howToPlayPanel, true);
    }

    public void CloseHowToPlay()
    {
        SafeSetActive(howToPlayPanel, false);
        ShowMainMenu(true);
    }

    void ShowMainMenu(bool on)
    {
        SafeSetActive(background, on);
        SafeSetActive(gameTitle, on);
        SafeSetActive(btnPlay, on);
        SafeSetActive(btnHowToPlay, on);
        SafeSetActive(btnOptions, on);
        SafeSetActive(btnExit, on);
    }

    void ApplyMenuInputState(bool toMenu)
    {
        if (!playerInput) playerInput = FindFirstObjectByType<PlayerInput>();
        Time.timeScale = 1f;
        if (toMenu)
        {
            if (cursorLockBehaviour) cursorLockBehaviour.enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (playerInput && !string.IsNullOrEmpty(uiMap))
            {
                var map = playerInput.actions != null ? playerInput.actions.FindActionMap(uiMap, false) : null;
                if (map != null) map.Enable();
                if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != uiMap) playerInput.SwitchCurrentActionMap(uiMap);
            }
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (cursorLockBehaviour) cursorLockBehaviour.enabled = true;
            if (playerInput && !string.IsNullOrEmpty(playerMap))
            {
                var map = playerInput.actions != null ? playerInput.actions.FindActionMap(playerMap, false) : null;
                if (map != null) map.Enable();
                if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != playerMap) playerInput.SwitchCurrentActionMap(playerMap);
            }
        }
    }

    void SafeSetActive(GameObject go, bool on)
    {
        if (go && go.activeSelf != on) go.SetActive(on);
    }

    Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform c in root)
        {
            var r = FindDeep(c, name);
            if (r) return r;
        }
        return null;
    }
}
