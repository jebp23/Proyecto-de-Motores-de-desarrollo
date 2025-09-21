using UnityEngine;
using TMPro;

public class LivesSystem : MonoBehaviour
{
    [SerializeField] int startingLives = 3;
    [SerializeField] TMP_Text livesText;
    int lives;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        lives = startingLives;
        UpdateUI();
    }

    void UpdateUI() { if (livesText) livesText.text = $"Lives X{lives}"; }

    public void LoseLife()
    {
        lives--;
        UpdateUI();

        // Ya NO reiniciamos la escena aquí; el respawn lo hace SanitySystem + SpawnPoint.
        if (lives <= 0)
            GameManager.I?.TriggerGameOver();
    }

    public void ResetLives()
    {
        lives = startingLives;
        UpdateUI();
    }
}
