using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Audio Settings")]
    [SerializeField] float sceneLoadDelay = 2f;
    [SerializeField] string firstLevelName = "Level1";

    [Header("UI Blocking")]
    [SerializeField] GraphicRaycaster raycaster;
    [SerializeField] Button[] menuButtons;

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

        if (!raycaster) raycaster = GetComponentInChildren<GraphicRaycaster>(true);
        if (menuButtons == null || menuButtons.Length == 0)
            menuButtons = GetComponentsInChildren<Button>(true);

        ApplyMenuInputState(true);
        ShowMainMenu(true);
    }

    void Start()
    {
        AudioManager.I?.EnterMainMenu();
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
        AudioManager.I?.EnterGame();
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
                var map = playerInput.actions?.FindActionMap(uiMap, false);
                if (map != null) map.Enable();
                if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != uiMap)
                    playerInput.SwitchCurrentActionMap(uiMap);
            }
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (cursorLockBehaviour) cursorLockBehaviour.enabled = true;
            if (playerInput && !string.IsNullOrEmpty(playerMap))
            {
                var map = playerInput.actions?.FindActionMap(playerMap, false);
                if (map != null) map.Enable();
                if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != playerMap)
                    playerInput.SwitchCurrentActionMap(playerMap);
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

    void BlockUI(bool on)
    {
        if (raycaster) raycaster.enabled = !on;

        if (menuButtons != null)
        {
            foreach (var b in menuButtons)
                if (b) b.interactable = !on;
        }
    }

    public void PlayNewGame()
    {
        StartCoroutine(PlayNewGameSequence());
    }

    System.Collections.IEnumerator PlayNewGameSequence()
    {
        BlockUI(true);
        AudioManager.I?.PlayVO_NewGame();
        yield return new WaitForSeconds(sceneLoadDelay);
        StartLevel(firstLevelName);
    }
}
