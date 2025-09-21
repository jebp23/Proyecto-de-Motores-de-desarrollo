using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    // Reiniciar nivel actual (y resetear vidas)
    public void OnClickRestart()
    {
        FindFirstObjectByType<LivesSystem>()?.ResetLives();
        Time.timeScale = 1f;
        GameManager.I?.RestartLevel();  // usa el método que ya tenés
    }

    // Ir al menú principal (poné el nombre de la escena de tu menú)
    public void OnClickMainMenu(string menuSceneName)
    {
        FindFirstObjectByType<LivesSystem>()?.ResetLives();
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
