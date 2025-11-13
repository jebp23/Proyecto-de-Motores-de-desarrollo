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
    public float voBlockTime = 3f;

    GraphicRaycaster raycaster;

    void Awake()
    {
        Instance = this;
        if (gameOverPanel) gameOverPanel.SetActive(false);

        raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
    }

    public void TriggerGameOver()
    {
        gameOverPanel.SetActive(true);

        retryButton.interactable = false;
        quitButton.interactable = false;

        if (raycaster) raycaster.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        AudioManager.I?.PlayVO_GameOver();

        StartCoroutine(Unlock());
    }

    IEnumerator Unlock()
    {
        yield return new WaitForSecondsRealtime(voBlockTime);

        retryButton.interactable = true;
        quitButton.interactable = true;

        if (raycaster) raycaster.enabled = true;
    }

    public void OnRetry()
    {
        LivesSystem.I.ResetLives();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuit()
    {
        LivesSystem.I.ResetLives();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
