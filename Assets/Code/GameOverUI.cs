using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
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
