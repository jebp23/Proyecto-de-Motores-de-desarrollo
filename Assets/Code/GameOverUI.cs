using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;
    public GameObject gameOverPanel;
    public Button retryButton;
    public Button quitButton;
    [SerializeField] private float voBlockTime = 3f;

    void Awake()
    {
        Instance = this;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void TriggerGameOver()
    {
        gameOverPanel.SetActive(true);
        retryButton.interactable = false;
        quitButton.interactable = false;
        AudioManager.I?.PlayVO_GameOver();
        StartCoroutine(UnlockInputAfterDelay());
    }

    private IEnumerator UnlockInputAfterDelay()
    {
        yield return new WaitForSeconds(voBlockTime);
        retryButton.interactable = true;
        quitButton.interactable = true;
    }

    public void OnRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuit()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
