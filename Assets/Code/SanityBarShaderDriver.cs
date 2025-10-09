using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SanityBarShaderDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider sanitySlider;
    [SerializeField] private Image fillImage;

    [Header("Glow Pulse")]
    [SerializeField] private float glowPeak = 3.0f;      // pico base
    [SerializeField] private float glowDecay = 5.0f;     // decaimiento (1/seg)
    [SerializeField] private float bandWidthMin = 0.02f; // 0.02–0.05 es lo normal
    [SerializeField] private float bandWidthMax = 0.06f; // ensanche momentáneo
    [SerializeField] private float bandWidthDecay = 5.0f;

    [Header("Orientation")]
    [SerializeField] private bool invertVertical = false;

    // Opcional: oculta el glow cuando la barra está casi llena (evita “glow fantasma” al iniciar)
    [Header("Behavior")]
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField, Range(0.95f, 1f)] private float fullCutoff = 0.995f;

    Material runtimeMat;
    float prev01 = 1f;
    float glow = 0f;
    float bandW;

    int idFill01, idGlow, idBand, idInvert;

    void Awake()
    {
        if (!fillImage) fillImage = GetComponent<Image>();
        if (!sanitySlider) sanitySlider = GetComponentInParent<Slider>();

        runtimeMat = Instantiate(fillImage.material);
        fillImage.material = runtimeMat;

        idFill01 = Shader.PropertyToID("_fill01");
        idGlow = Shader.PropertyToID("_GlowStrength");
        idBand = Shader.PropertyToID("_BandWidth");
        idInvert = Shader.PropertyToID("_InvertVertical");

        bandW = bandWidthMin;
        runtimeMat.SetFloat(idInvert, invertVertical ? 1f : 0f);
        runtimeMat.SetFloat(idGlow, 0f);

        // Sync inicial inmediato (evita ver la banda “a mitad” al empezar)
        float max = Mathf.Max(0.0001f, sanitySlider ? sanitySlider.maxValue : 1f);
        prev01 = sanitySlider ? Mathf.Clamp01(sanitySlider.value / max) : 1f;
        runtimeMat.SetFloat(idFill01, prev01);
        runtimeMat.SetFloat(idBand, bandW);
    }

    void OnEnable()
    {
        if (sanitySlider)
            sanitySlider.onValueChanged.AddListener(OnSliderChanged);
    }
    void OnDisable()
    {
        if (sanitySlider)
            sanitySlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    void OnSliderChanged(float _)
    {
        // Reacción inmediata en el mismo frame del cambio
        float cur01 = Mathf.Clamp01(sanitySlider.value / Mathf.Max(0.0001f, sanitySlider.maxValue));
        runtimeMat.SetFloat(idFill01, cur01);

        float delta = prev01 - cur01;
        if (delta > 0.0001f)
        {
            // Pulso proporcional a la caída
            glow = Mathf.Max(glow, glowPeak) + delta * 6.0f;
            bandW = Mathf.Max(bandW, bandWidthMax + delta * 0.12f);
        }
        prev01 = cur01;
    }

    void Update()
    {
        if (!runtimeMat) return;

        if (hideWhenFull && prev01 >= fullCutoff)
        {
            glow = 0f;
            runtimeMat.SetFloat(idGlow, 0f);
        }
        else if (glow > 0f)
        {
            glow = Mathf.Max(0f, glow - glowDecay * Time.unscaledDeltaTime);
            runtimeMat.SetFloat(idGlow, glow);
        }

        if (bandW > bandWidthMin)
        {
            bandW = Mathf.Lerp(bandW, bandWidthMin, bandWidthDecay * Time.unscaledDeltaTime);
            runtimeMat.SetFloat(idBand, bandW);
        }
    }
}
