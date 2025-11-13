using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VictoryUI : MonoBehaviour
{
    [SerializeField] GameObject root;
    [SerializeField] string mainMenuSceneName = "MainMenu";
    [SerializeField] PlayerInput playerInput;
    [SerializeField] string uiMap = "UI";
    [SerializeField] string playerMap = "Player";
    [SerializeField] CursorLock cursorLock;
    [SerializeField] private float voBlockTime = 4f;

    CanvasGroup group;
    Button[] buttons;
    EventSystem ev;

    void OnEnable()
    {
        ev = EventSystem.current;

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

        if (root)
        {
            group = root.GetComponent<CanvasGroup>();
            if (!group) group = root.AddComponent<CanvasGroup>();
        }

        buttons = root ? root.GetComponentsInChildren<Button>(true) : null;

        Time.timeScale = 0f;

        if (group)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        if (ev) ev.enabled = false;

        AudioManager.I.StopAllMusicNow();
        AudioManager.I?.PlayVO_Victory();

        StartCoroutine(Unlock());
    }

    IEnumerator Unlock()
    {
        yield return new WaitForSecondsRealtime(voBlockTime);

        if (group)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        if (ev) ev.enabled = true;
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

        var pause = FindFirstObjectByType<LogicaOpciones>(FindObjectsInactive.Include);
        if (pause != null) pause.BlockPause(false);
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
