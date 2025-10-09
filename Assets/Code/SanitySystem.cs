using UnityEngine;
using UnityEngine.UI;

public class SanitySystem : MonoBehaviour
{
    [SerializeField] private Slider sanitySlider;
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private AudioSource deathSfxSource;
    [SerializeField] private AudioClip deathClip;

    // --- NUEVO: suavizado de UI ---
    [Header("UI Smoothing")]
    [SerializeField] private bool smoothUI = true;
    [SerializeField, Range(0.05f, 0.35f)] private float uiSmoothTime = 0.15f;
    float displaySanity;         // valor mostrado
    float displayVel;            // vel. interna de SmoothDamp

    private float currentSanity;
    private bool hasDepleted;

    private void Awake()
    {
        currentSanity = maxSanity;
        displaySanity = currentSanity;

        if (sanitySlider)
        {
            sanitySlider.wholeNumbers = false;                    // evita “saltos”
            sanitySlider.maxValue = maxSanity;
            sanitySlider.value = displaySanity;
        }

        if (deathSfxSource)
        {
            deathSfxSource.playOnAwake = false;
            deathSfxSource.loop = false;
        }
    }

    private void Update()
    {
        // --- NUEVO: mover suavemente el valor mostrado hacia el real ---
        if (!sanitySlider) return;

        if (smoothUI)
        {
            displaySanity = Mathf.SmoothDamp(
                displaySanity, currentSanity, ref displayVel,
                Mathf.Max(0.01f, uiSmoothTime)
            );
            sanitySlider.value = displaySanity;
        }
        else
        {
            sanitySlider.value = currentSanity;
        }
    }

    public void TakeDamage(float amount)
    {
        if (hasDepleted) return;
        currentSanity = Mathf.Clamp(currentSanity - amount, 0f, maxSanity);

        // si NO suavizas, refleja directo; si sí suavizas, Update() se encarga
        if (!smoothUI && sanitySlider) sanitySlider.value = currentSanity;

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
        if (!smoothUI && sanitySlider) sanitySlider.value = currentSanity;
        hasDepleted = false;
        // También sincronizamos el mostrado para evitar “saltos” al entrar a escena:
        displaySanity = currentSanity;
        displayVel = 0f;
    }
}
