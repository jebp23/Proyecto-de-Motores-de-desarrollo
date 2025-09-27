using UnityEngine;
using UnityEngine.UI;

public class SanitySystem : MonoBehaviour
{
    [SerializeField] private Slider sanitySlider;
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private AudioSource deathSfxSource;
    [SerializeField] private AudioClip deathClip;

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
        if (deathSfxSource)
        {
            deathSfxSource.playOnAwake = false;
            deathSfxSource.loop = false;
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
            if (deathClip)
            {
                if (deathSfxSource) deathSfxSource.PlayOneShot(deathClip);
                else AudioManager.I?.PlayOneShot(deathClip, 1f);
            }
            LivesSystem.I?.LoseLife();
        }
    }

    public void RestoreFull()
    {
        currentSanity = maxSanity;
        if (sanitySlider) sanitySlider.value = currentSanity;
        hasDepleted = false;
    }
}
