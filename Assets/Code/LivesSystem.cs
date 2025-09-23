using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LivesSystem : MonoBehaviour
{
    public static LivesSystem I { get; private set; }


    [SerializeField] int startingLives = 3;
    [SerializeField] TMP_Text livesText;               
    [SerializeField] string livesTextTag = "LivesText"; 
    [SerializeField] bool restartSceneOnLoseLife = false;

    int lives;

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
        lives = Mathf.Max(0, lives - 1);
        UpdateUI();

        if (lives <= 0)
        {
            GameManager.I?.TriggerGameOver();
            return;
        }


        if (restartSceneOnLoseLife) GameManager.I?.RestartLevel();
        else GameEvents.RaiseLevelRestart(); 
    }

    public void ResetLives()
    {
        lives = startingLives;
        UpdateUI();
    }

    public int CurrentLives => lives;
}
