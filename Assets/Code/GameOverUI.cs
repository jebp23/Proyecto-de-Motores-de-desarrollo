using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;
    public GameObject gameOverPanel;
    public Button retryButton;
    public Button quitButton;
    [SerializeField] private float voBlockTime = 3f;
    CanvasGroup group;
    EventSystem ev;

    void Awake()
    {
        Instance = this;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Start()
    {
        ev = EventSystem.current;
        if (gameOverPanel) group = gameOverPanel.GetComponent<CanvasGroup>();
        if (group == null && gameOverPanel) group = gameOverPanel.AddComponent<CanvasGroup>();
    }

    public void TriggerGameOver()
    {
        gameOverPanel.SetActive(true);

        if (group)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        if (ev) ev.enabled = false;

        AudioManager.I?.PlayVO_GameOver();

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

    public void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
