using UnityEngine;
using UnityEngine.UI;

public class ColdFourSidesUI : MonoBehaviour, IColdUI
{
    public static ColdFourSidesUI I { get; private set; } // opcional, útil para debug

    [Header("Sides")]
    [SerializeField] Image topImg;
    [SerializeField] Image bottomImg;
    [SerializeField] Image leftImg;
    [SerializeField] Image rightImg;
    [SerializeField] CanvasGroup group;

    [Header("Progress (0..1)")]
    [SerializeField, Range(0f, 1f)] float current;
    [SerializeField, Range(0f, 1f)] float target;
    [SerializeField] float lerpSpeed = 1.5f;

    [Header("Alpha sync")]
    [SerializeField] AnimationCurve alphaByFill = AnimationCurve.Linear(0, 0.1f, 1, 1f);

    [Header("SFX thresholds")]
    [SerializeField, Range(0f, 1f)] float exhaleAt = 0.333f;   // 1/3
    [SerializeField, Range(0f, 1f)] float sneezeAt = 0.5f;     // 1/2 (el estornudo reemplaza la tos)
    [SerializeField] AudioClip exhaleClip;
    [SerializeField, Range(0f, 1f)] float exhaleVolume = 1f;
    [SerializeField] AudioClip sneezeClip;
    [SerializeField, Range(0f, 1f)] float sneezeVolume = 1f;

    [Header("Noise push (0..1)")]
    [SerializeField, Range(0f, 1f)] float exhaleNoise = 0.06f;
    [SerializeField, Range(0f, 1f)] float sneezeNoise = 0.16f;

    NoiseMeter noise; // publica GameEvents → lo consume tu barra y el detector de sonido. 
    bool exhaleTrig, sneezeTrig;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        noise = FindFirstObjectByType<NoiseMeter>();
        Prepare(topImg, Image.FillMethod.Vertical, 1); // Top
        Prepare(bottomImg, Image.FillMethod.Vertical, 0); // Bottom
        Prepare(leftImg, Image.FillMethod.Horizontal, 0); // Left
        Prepare(rightImg, Image.FillMethod.Horizontal, 1); // Right
        if (group) { group.blocksRaycasts = false; group.interactable = false; }
    }

    void Prepare(Image img, Image.FillMethod method, int origin)
    {
        if (!img) return;
        img.type = Image.Type.Filled;
        img.fillMethod = method;
        img.fillOrigin = origin;
        img.fillAmount = 0f;
    }

    void Update()
    {
        current = Mathf.MoveTowards(current, target, lerpSpeed * Time.unscaledDeltaTime);
        ApplyVisuals(current);
        CheckSfx(current);
    }

    public void SetTarget01(float v)
    {
        target = Mathf.Clamp01(v);
        if (target < exhaleAt) exhaleTrig = false;
        if (target < sneezeAt) sneezeTrig = false;
    }

    public void AddTargetDelta(float d) => SetTarget01(target + d);

    void ApplyVisuals(float p)
    {
        if (topImg) topImg.fillAmount = p;
        if (bottomImg) bottomImg.fillAmount = p;
        if (leftImg) leftImg.fillAmount = p;
        if (rightImg) rightImg.fillAmount = p;
        if (group) group.alpha = Mathf.Clamp01(alphaByFill.Evaluate(p));
    }

    void CheckSfx(float p)
    {
        if (!exhaleTrig && p >= exhaleAt)
        {
            exhaleTrig = true;
            if (exhaleClip) AudioManager.I?.PlayOneShot(exhaleClip, exhaleVolume); // :contentReference[oaicite:1]{index=1}
            if (noise) noise.AddNormalizedNoise(exhaleNoise);                      // :contentReference[oaicite:2]{index=2}
        }
        if (!sneezeTrig && p >= sneezeAt)
        {
            sneezeTrig = true;
            if (sneezeClip) AudioManager.I?.PlayOneShot(sneezeClip, sneezeVolume); // :contentReference[oaicite:3]{index=3}
            if (noise) noise.AddNormalizedNoise(sneezeNoise);                       // :contentReference[oaicite:4]{index=4}
        }
    }

    public void ForceClear()
    {
        current = target = 0f;
        exhaleTrig = sneezeTrig = false;
        ApplyVisuals(0f);
    }

    public float Current01 => current;
    public float Target01 => target;
}
