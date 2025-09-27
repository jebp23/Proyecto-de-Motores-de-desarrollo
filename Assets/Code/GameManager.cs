using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public enum GameState { Playing, Paused, Victory, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [SerializeField] GameObject victoryPanel;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text progressText;
    [SerializeField] int totalNotes = 4;

    [SerializeField] DeathFadeController deathFader;
    [SerializeField] string deathSfxObjectName = "DeathSFXSource";
    [SerializeField] string deathSfxTag = "DeathSFXSource";
    [SerializeField] float respawnGraceSeconds = 2f;
    [SerializeField] float spawnClearRadius = 5f;

    HashSet<Document> collectedSet = new HashSet<Document>();
    int collectedNotes;
    GameState state = GameState.Playing;
    bool respawning;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.OnLevelRestart += OnLevelRestart;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEvents.OnLevelRestart -= OnLevelRestart;
    }

    void Start() { BootstrapScene(); }
    void OnSceneLoaded(Scene s, LoadSceneMode m) { BootstrapScene(); }

    void BootstrapScene()
    {
        state = GameState.Playing;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (!deathFader) deathFader = FindFirstObjectByType<DeathFadeController>(FindObjectsInactive.Include);
        if (victoryPanel) victoryPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        var pause = FindFirstObjectByType<LogicaOpciones>(FindObjectsInactive.Include);
        if (pause != null) pause.BlockPause(false);
        if (deathFader) StartCoroutine(deathFader.FadeIn(null, true));
        AudioManager.I?.FadeInAll(deathFader ? deathFader.DefaultFadeIn : 0.35f);
        RecountCollected();
        UpdateProgressUI();
    }

    public void RestartLevel() { StartCoroutine(RestartLevelCo()); }

    IEnumerator RestartLevelCo()
    {
        var pause = FindFirstObjectByType<LogicaOpciones>(FindObjectsInactive.Include);
        if (pause != null) { pause.EsconderOpciones(); pause.BlockPause(true); }
        Time.timeScale = 1f;
        if (!deathFader) deathFader = FindFirstObjectByType<DeathFadeController>(FindObjectsInactive.Include);
        if (deathFader) yield return deathFader.FadeOut(null, true);
        AudioManager.I?.FadeOutAll(0.3f);
        var current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void TriggerGameOver()
    {
        state = GameState.GameOver;
        if (gameOverPanel) gameOverPanel.SetActive(true);
        var pause = FindFirstObjectByType<LogicaOpciones>(FindObjectsInactive.Include);
        if (pause != null) { pause.EsconderOpciones(); pause.BlockPause(true); }
        AudioManager.I?.FadeOutAll(0.4f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void TriggerVictory()
    {
        state = GameState.Victory;
        if (victoryPanel) victoryPanel.SetActive(true);
        var pause = FindFirstObjectByType<LogicaOpciones>(FindObjectsInactive.Include);
        if (pause != null) pause.BlockPause(true);
        Time.timeScale = 0f;
    }

    void OnLevelRestart()
    {
        if (!respawning) StartCoroutine(RespawnSequence());
    }

    IEnumerator RespawnSequence()
    {
        respawning = true;
        Time.timeScale = 1f;

        var pause = FindFirstObjectByType<LogicaOpciones>(FindObjectsInactive.Include);
        if (pause != null) { pause.EsconderOpciones(); pause.BlockPause(true); }

        if (!deathFader) deathFader = FindFirstObjectByType<DeathFadeController>(FindObjectsInactive.Include);
        AudioSource except = ResolveDeathSfxSource();
        AudioManager.I?.FadeOutAll(0.25f, except);
        if (deathFader) yield return deathFader.FadeOut(null, true);
        AudioManager.I?.StopAll(except);

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            SpawnPoint.I?.RespawnPlayer(player);
            StartCoroutine(TempIgnoreCollisionsWithEnemies(player, respawnGraceSeconds));
        }

        ClearAroundSpawn(player != null ? player.transform.position : Vector3.zero);
        SuppressAllEnemies(respawnGraceSeconds);

        yield return new WaitForSecondsRealtime(0.15f);

        if (deathFader) yield return deathFader.FadeIn(null, true);
        AudioManager.I?.ReplayStopped();
        AudioManager.I?.FadeInAll(deathFader ? deathFader.DefaultFadeIn : 0.35f);

        if (pause != null) pause.BlockPause(false);
        respawning = false;
    }

    IEnumerator TempIgnoreCollisionsWithEnemies(GameObject player, float seconds)
    {
        if (player == null) yield break;
        var pcols = player.GetComponentsInChildren<Collider>(true);
        var enemies = FindObjectsByType<EnemyMonster>(FindObjectsSortMode.None);
        var pairs = new List<(Collider, Collider)>();
        for (int i = 0; i < enemies.Length; i++)
        {
            var ecols = enemies[i].GetComponentsInChildren<Collider>(true);
            for (int a = 0; a < pcols.Length; a++)
                for (int b = 0; b < ecols.Length; b++)
                {
                    Physics.IgnoreCollision(pcols[a], ecols[b], true);
                    pairs.Add((pcols[a], ecols[b]));
                }
        }
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, seconds));
        for (int i = 0; i < pairs.Count; i++)
            Physics.IgnoreCollision(pairs[i].Item1, pairs[i].Item2, false);
    }

    void ClearAroundSpawn(Vector3 center)
    {
        var enemies = FindObjectsByType<EnemyMonster>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            if (!e) continue;
            float d = Vector3.Distance(center, e.transform.position);
            if (d < spawnClearRadius) e.WarpAwayFrom(center, spawnClearRadius + 1f);
        }
    }

    void SuppressAllEnemies(float seconds)
    {
        var enemies = FindObjectsByType<EnemyMonster>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++) enemies[i]?.SuppressFor(seconds);
    }

    AudioSource ResolveDeathSfxSource()
    {
        var byName = GameObject.Find(deathSfxObjectName)?.GetComponent<AudioSource>();
        if (byName) return byName;
        if (!string.IsNullOrEmpty(deathSfxTag))
        {
            var byTag = GameObject.FindWithTag(deathSfxTag)?.GetComponent<AudioSource>();
            if (byTag) return byTag;
        }
        return null;
    }

    void RecountCollected()
    {
        collectedSet.Clear();
        collectedNotes = 0;
        var docs = Object.FindObjectsOfType<Document>(true);
        for (int i = 0; i < docs.Length; i++)
        {
            var d = docs[i];
            if (d && d.collected && collectedSet.Add(d)) collectedNotes++;
        }
    }

    void UpdateProgressUI()
    {
        if (progressText) progressText.text = $"{collectedNotes}/{totalNotes}";
    }

    public void DocumentCollected(Document doc)
    {
        if (doc == null) return;
        if (collectedSet.Add(doc)) collectedNotes++;
        UpdateProgressUI();
        if (collectedNotes >= totalNotes) TriggerVictory();
    }

    public GameState CurrentState => state;
}
