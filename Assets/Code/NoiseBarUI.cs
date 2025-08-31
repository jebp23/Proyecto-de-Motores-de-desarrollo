using UnityEngine;
using UnityEngine.UI;

public class NoiseBarUI : MonoBehaviour
{
    [SerializeField] private Slider noiseSlider;
    [SerializeField] private Image fillImage;

    private void OnEnable() => GameEvents.OnNoiseChanged += UpdateNoise;
    private void OnDisable() => GameEvents.OnNoiseChanged -= UpdateNoise;

    private void UpdateNoise(float normalizedValue)
    {
        if (noiseSlider != null)
            noiseSlider.value = normalizedValue;

        if (fillImage != null)
            fillImage.color = new Color32(0xA4, 0x20, 0x20, 0xFF); // siempre rojo #A42020
    }
}
