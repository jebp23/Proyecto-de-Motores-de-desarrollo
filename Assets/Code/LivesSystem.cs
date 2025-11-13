using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LivesSystem : MonoBehaviour
{
    public static LivesSystem I { get; private set; }

    [SerializeField] int startingLives = 3;
    [SerializeField] TMP_Text livesText;
    [SerializeField] string livesTextTag = "LivesText";
    [SerializeField] bool restartSceneOnLoseLife = false;
    [SerializeField] float deathGroanDuration = 1.2f;

    int lives;
    bool busy;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        lives = startingLives;
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void Start() { RebindUI(); UpdateUI(); }
    void OnSceneLoaded(Scene s, LoadSceneMode m) { RebindUI(); UpdateUI(); }

    void RebindUI()
    {
        if (livesText == null && !string.IsNullOrEmpty(livesTextTag))
        {
            var go = GameObject.FindWithTag(livesTextTag);
            livesText = go ? go.GetComponent<TMP_Text>() : null;
        }
    }

    void UpdateUI()
    {
        if (livesText) livesText.text = $"VIDAS X{lives}";
    }

    public void LoseLife()
    {
        if (busy) return;
        busy = true;

        lives = Mathf.Max(0, lives - 1);
        UpdateUI();

        if (lives == 2 || lives == 1)
        {
            StartCoroutine(DeathGroanRespawn());
            return;
        }

        if (lives <= 0)
        {
            AudioManager.I?.PlayVO_GameOver();
            GameManager.I?.TriggerGameOver();
            busy = false;
            return;
        }

        if (restartSceneOnLoseLife)
        {
            GameManager.I?.RestartLevel();
            busy = false;
        }
        else
        {
            GameEvents.RaiseLevelRestart();
            busy = false;
        }
    }

    IEnumerator DeathGroanRespawn()
    {
        AudioManager.I?.PlayDeathGroan();
        yield return new WaitForSecondsRealtime(deathGroanDuration);

        if (restartSceneOnLoseLife)
            GameManager.I?.RestartLevel();
        else
            GameEvents.RaiseLevelRestart();

        busy = false;
    }

    public void ResetLives()
    {
        lives = startingLives;
        UpdateUI();
    }

    public int CurrentLives => lives;
}
