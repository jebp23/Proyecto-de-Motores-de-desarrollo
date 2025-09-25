using UnityEngine;
using UnityEngine.UI;

public class SanitySystem : MonoBehaviour
{
    [SerializeField] private Slider sanitySlider;
    [SerializeField] private float maxSanity = 100f;

    private float currentSanity;
    private bool hasDepleted; 

    private void Awake()
    {
        currentSanity = maxSanity;
        if (sanitySlider)
        {
            sanitySlider.maxValue = maxSanity;
            sanitySlider.value = currentSanity;
        }
    }

    public void TakeDamage(float amount)
    {
        if (hasDepleted) return;

        currentSanity = Mathf.Clamp(currentSanity - amount, 0f, maxSanity);
        if (sanitySlider) sanitySlider.value = currentSanity;

        if (currentSanity <= 0f)
        {
            hasDepleted = true;

            LivesSystem.I?.LoseLife();


            if (GameManager.I == null || GameManager.I.CurrentState != GameState.GameOver)
            {
                DeathFadeController.I?.PlayDeathSequence(gameObject);
            }
        }
    }


    public void RestoreFull()
    {
        currentSanity = maxSanity;
        if (sanitySlider) sanitySlider.value = currentSanity;
        hasDepleted = false;
    }
}
