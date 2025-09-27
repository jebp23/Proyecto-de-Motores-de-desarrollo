using System.Collections;
using UnityEngine;

public class DeathFadeController : MonoBehaviour
{
    public static DeathFadeController I { get; private set; }

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float defaultFadeOut = 0.6f;
    [SerializeField] float defaultFadeIn = 0.6f;
    [SerializeField] GameObject[] uiRootsToHide;
    [SerializeField] Canvas targetCanvas;

    void Awake()
    {
        I = this;
        if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        if (!targetCanvas && canvasGroup) targetCanvas = canvasGroup.GetComponentInParent<Canvas>(true);
        if (targetCanvas)
        {
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            targetCanvas.overrideSorting = true;
            targetCanvas.sortingOrder = 32760;
        }
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.gameObject.SetActive(true);
        }
    }

    public IEnumerator FadeOut(float? duration = null, bool hideUI = true)
    {
        if (!canvasGroup) yield break;
        if (hideUI && uiRootsToHide != null) foreach (var go in uiRootsToHide) if (go) go.SetActive(false);
        float d = duration.HasValue ? duration.Value : defaultFadeOut;
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        float a0 = canvasGroup.alpha;
        float t = 0f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(a0, 1f, t / Mathf.Max(0.01f, d));
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    public IEnumerator FadeIn(float? duration = null, bool showUI = true)
    {
        if (!canvasGroup) yield break;
        float d = duration.HasValue ? duration.Value : defaultFadeIn;
        float a0 = canvasGroup.alpha;
        float t = 0f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(a0, 0f, t / Mathf.Max(0.01f, d));
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        if (showUI && uiRootsToHide != null) foreach (var go in uiRootsToHide) if (go) go.SetActive(true);
    }

    public float DefaultFadeOut => defaultFadeOut;
    public float DefaultFadeIn => defaultFadeIn;
}
