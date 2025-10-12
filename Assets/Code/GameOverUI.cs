using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] GameObject root;

    void Update()
    {
        if (root && !root.activeInHierarchy) return;
        if (Keyboard.current == null) return;
        if (Keyboard.current.rKey.wasPressedThisFrame) OnClickRestart();
        if (Keyboard.current.mKey.wasPressedThisFrame) OnClickMainMenu("MainMenu");
    }

    public void OnClickRestart()
    {
        FindFirstObjectByType<LivesSystem>()?.ResetLives();
        Time.timeScale = 1f;
        GameManager.I?.RestartLevel();
    }

    public void OnClickMainMenu(string menuSceneName)
    {
        FindFirstObjectByType<LivesSystem>()?.ResetLives();
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
