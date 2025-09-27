using System;
using System.Collections;
using UnityEngine;

public class DeathFadeController : MonoBehaviour
{
    public static DeathFadeController I { get; private set; }

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float defaultFadeOut = 0.6f;
    [SerializeField] float defaultBlackHold = 0.6f;
    [SerializeField] float defaultFadeIn = 0.6f;

    public static event Action OnBlackHoldFinished;

    void Awake()
    {
        I = this;
        if (!canvasGroup) canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.gameObject.SetActive(true);
        }
    }

    public IEnumerator FadeOut(float? duration = null)
    {
        if (!canvasGroup) yield break;
        float d = duration.HasValue ? Mathf.Max(0f, duration.Value) : Mathf.Max(0f, defaultFadeOut);
        if (d <= 0f) { canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = true; yield break; }
        float a0 = canvasGroup.alpha;
        float t = 0f;
        canvasGroup.blocksRaycasts = true;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(a0, 1f, t / d);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    public IEnumerator BlackHold(float? hold = null)
    {
        if (!canvasGroup) yield break;
        float h = hold.HasValue ? Mathf.Max(0f, hold.Value) : Mathf.Max(0f, defaultBlackHold);
        float t = 0f;
        while (t < h)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        OnBlackHoldFinished?.Invoke();
    }

    public IEnumerator FadeIn(float? duration = null)
    {
        if (!canvasGroup) yield break;
        float d = duration.HasValue ? Mathf.Max(0f, duration.Value) : Mathf.Max(0f, defaultFadeIn);
        if (d <= 0f) { canvasGroup.alpha = 0f; canvasGroup.blocksRaycasts = false; yield break; }
        float a0 = canvasGroup.alpha;
        float t = 0f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(a0, 0f, t / d);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public IEnumerator FadeOutHoldIn(float? fadeOut = null, float? blackHold = null, float? fadeIn = null)
    {
        yield return FadeOut(fadeOut);
        yield return BlackHold(blackHold);
        yield return FadeIn(fadeIn);
    }

    public void HoldThenFadeIn(float? hold = null, float? fadeIn = null)
    {
        StartCoroutine(HoldThenFadeInCo(hold, fadeIn));
    }

    IEnumerator HoldThenFadeInCo(float? hold, float? fadeIn)
    {
        yield return BlackHold(hold);
        yield return FadeIn(fadeIn);
    }

    public float DefaultFadeOut { get => defaultFadeOut; set => defaultFadeOut = Mathf.Max(0f, value); }
    public float DefaultBlackHold { get => defaultBlackHold; set => defaultBlackHold = Mathf.Max(0f, value); }
    public float DefaultFadeIn { get => defaultFadeIn; set => defaultFadeIn = Mathf.Max(0f, value); }
}
