using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    // Reiniciar nivel actual (y resetear vidas)
    public void OnClickRestart()
    {
        FindFirstObjectByType<LivesSystem>()?.ResetLives();
        Time.timeScale = 1f;
        GameManager.I?.RestartLevel();  // usa el m�todo que ya ten�s
    }

    // Ir al men� principal (pon� el nombre de la escena de tu men�)
    public void OnClickMainMenu(string menuSceneName)
    {
        FindFirstObjectByType<LivesSystem>()?.ResetLives();
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
