using UnityEngine;
using UnityEngine.UI;

public class SanitySystem : MonoBehaviour
{
    [SerializeField] private Slider sanitySlider;
    [SerializeField] private float maxSanity = 100f;
    private bool hasDepleted = false;

    private float currentSanity;

    private void Awake()
    {
        currentSanity = maxSanity;
        sanitySlider.maxValue = maxSanity;
        sanitySlider.value = currentSanity;
    }

    // SanitySystem.cs

    // SanitySystem.cs

    public void TakeDamage(float amount)
    {
        if (hasDepleted) return;

        currentSanity -= amount;
        currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);
        sanitySlider.value = currentSanity;

        if (currentSanity <= 0)
        {
            var lives = FindFirstObjectByType<LivesSystem>();
            if (lives != null) lives.LoseLife();

            var gm = FindFirstObjectByType<GameManager>();
            bool isGameOver = (gm != null && gm.CurrentState == GameState.GameOver);

            if (!isGameOver)
                SpawnPoint.I?.RespawnPlayer(gameObject);

            hasDepleted = true; // ← evita múltiples descuentos en el mismo “death”
        }
    }

    public void RestoreFull()
    {
        currentSanity = maxSanity;
        sanitySlider.value = currentSanity;
        hasDepleted = false; // ← volvemos a habilitar daño luego del respawn
    }

}
