using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public enum GameState { Playing, Paused, Victory, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("UI")]
    [SerializeField] GameObject victoryPanel;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text progressText;

    [Header("Docs")]
    [SerializeField] Document[] allDocuments;

    private int collectedDocs = 0;
    private GameState state = GameState.Playing;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void Start() { BootstrapScene(); }
    void OnSceneLoaded(Scene s, LoadSceneMode m) { BootstrapScene(); }

    void BootstrapScene()
    {
        state = GameState.Playing;
        Time.timeScale = 1f;

        if (victoryPanel) victoryPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);

        if (allDocuments == null || allDocuments.Length == 0)
            allDocuments = FindObjectsOfType<Document>(true);

        collectedDocs = 0;
        UpdateProgressUI();
    }

    void UpdateProgressUI()
    {
        if (progressText) progressText.text = $"{collectedDocs}/{(allDocuments?.Length ?? 0)}";
    }

    public void DocumentCollected(Document d)
    {
        collectedDocs++;
        UpdateProgressUI();
        if (collectedDocs >= (allDocuments?.Length ?? 0)) TriggerVictory();
    }

    public void TriggerGameOver()
    {
        state = GameState.GameOver;
        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    void TriggerVictory()
    {
        state = GameState.Victory;
        if (victoryPanel) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public GameState CurrentState => state;
}
