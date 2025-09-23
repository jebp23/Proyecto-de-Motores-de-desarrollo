using UnityEngine;
using TMPro;

public enum GameState
{
    Playing,
    Paused,
    Victory,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text progressText; 

    private Document[] allDocuments;
    private int collectedDocs = 0;
    private GameState state = GameState.Playing;

    private void Awake()
    {
        // Singleton
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Buscar todos los documentos en la escena
        allDocuments = FindObjectsOfType<Document>();
        Debug.Log($"[GameManager] Documentos totales: {allDocuments.Length}");

        UpdateProgressUI();
    }

    public void DocumentCollected(Document doc)
    {
        if (!doc.collected)
        {
            doc.collected = true;
            collectedDocs++;
            Debug.Log($"[GameManager] Documento recogido ({collectedDocs}/{allDocuments.Length})");

            UpdateProgressUI();

            if (collectedDocs >= allDocuments.Length)
            {
                TriggerVictory();
            }
        }
    }

    private void UpdateProgressUI()
    {
        if (progressText)
            progressText.text = $"{collectedDocs}/{allDocuments.Length}";
    }

    private void TriggerVictory()
    {
        state = GameState.Victory;
        Debug.Log("[GameManager] ¡Victoria! Todos los documentos fueron leídos.");
        if (victoryPanel) victoryPanel.SetActive(true);

        // Opcional: detener el tiempo
        Time.timeScale = 0f;
    }

    public GameState CurrentState => state;
}
