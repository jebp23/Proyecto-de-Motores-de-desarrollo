using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathFadeController : MonoBehaviour
{
    public static DeathFadeController I { get; private set; }

    [Header("Canvases")]
    [SerializeField] private Canvas deathCanvas;
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private bool autoSortAboveUI = true;
    [SerializeField] private int orderOffset = 100;

    [Header("UI Elements")]
    [SerializeField] private GameObject panelRoot;     // hijo con Image negro + CanvasGroup (INACTIVO por defecto)
    [SerializeField] private CanvasGroup fadeGroup;    // CanvasGroup del panel negro
    [SerializeField] private Image blackImage;         // Image negro full screen (raycastTarget = true)

    [Header("Timings")]
    [SerializeField, Min(0f)] private float fadeInTime = 0.25f;
    [SerializeField, Min(0f)] private float holdTime = 1.0f;  // duración del blackout
    [SerializeField, Min(0f)] private float fadeOutTime = 0.25f;
    [SerializeField] private bool hideUICanvasDuringBlackout = false;

    [Header("Audio")]
    [SerializeField] private AudioClip deathSfx;
    [SerializeField] private AudioSource sfxSource; // opcional

    private bool _isRunning;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        if (autoSortAboveUI && deathCanvas)
        {
            deathCanvas.overrideSorting = true;
            int baseOrder = uiCanvas ? uiCanvas.sortingOrder : 0;
            deathCanvas.sortingOrder = baseOrder + orderOffset;
        }

        if (fadeGroup)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
        }
        if (blackImage) blackImage.raycastTarget = true; // bloquea clics
        if (panelRoot && panelRoot.activeSelf) panelRoot.SetActive(false); // INACTIVO por defecto
    }

    public void PlayDeathSequence(GameObject playerGO)
    {
        if (_isRunning || playerGO == null) return;
        StartCoroutine(DeathCR(playerGO));
    }

    private IEnumerator DeathCR(GameObject playerGO)
    {
        _isRunning = true;

        // Mostrar panel y bloquear input
        if (panelRoot && !panelRoot.activeSelf) panelRoot.SetActive(true);
        SetPlayerInput(playerGO, false);

        // SFX
        if (deathSfx)
        {
            if (sfxSource) sfxSource.PlayOneShot(deathSfx);
            else AudioManager.I?.PlayGrowl(); // o usa tu AudioManager si preferís otro método:contentReference[oaicite:4]{index=4}
        }

        // Opcional: ocultar UICanvas entero mientras dura el blackout
        if (hideUICanvasDuringBlackout && uiCanvas) uiCanvas.gameObject.SetActive(false);

        // Fade In
        yield return StartCoroutine(FadeTo(1f, fadeInTime));

        // Reubicar jugador (si no hay Game Over)
        if (GameManager.I == null || GameManager.I.CurrentState != GameState.GameOver)
        {
            SpawnPoint.I?.RespawnPlayer(playerGO); // tu respawn del jugador
        }

        // Mantener negro
        float t = 0f;
        while (t < holdTime) { t += Time.unscaledDeltaTime; yield return null; }

        // Fade Out
        yield return StartCoroutine(FadeTo(0f, fadeOutTime));

        // Restaurar UI + input si no hay GameOver
        if (hideUICanvasDuringBlackout && uiCanvas) uiCanvas.gameObject.SetActive(true);
        if (GameManager.I == null || GameManager.I.CurrentState != GameState.GameOver)
            SetPlayerInput(playerGO, true);

        if (panelRoot) panelRoot.SetActive(false);
        _isRunning = false;
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        if (!fadeGroup) yield break;
        fadeGroup.blocksRaycasts = true;
        fadeGroup.interactable = true;

        float start = fadeGroup.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
            fadeGroup.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }
        fadeGroup.alpha = target;

        if (Mathf.Approximately(target, 0f))
        {
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
        }
    }

    private void SetPlayerInput(GameObject playerGO, bool on)
    {
        var ctrl = playerGO.GetComponent<PlayerRigidBodyController>();
        if (ctrl != null)
        {
            ctrl.SetInputEnabled(on); // añade este método al controlador del jugador
        }
        var flashlight = playerGO.GetComponentInChildren<FlashlightController>(true);
        if (flashlight) flashlight.enabled = on;
    }
}
