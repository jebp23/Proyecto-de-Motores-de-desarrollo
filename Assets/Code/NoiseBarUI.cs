using UnityEngine;
using UnityEngine.UI;

public class NoiseBarUI : MonoBehaviour
{
    [SerializeField] Slider noiseSlider;
    [SerializeField] Image fillImage;

    void OnEnable() { GameEvents.OnNoiseChanged += UpdateNoise; }
    void OnDisable() { GameEvents.OnNoiseChanged -= UpdateNoise; }

    void UpdateNoise(float normalizedValue)
    {
        if (noiseSlider) noiseSlider.value = normalizedValue;
        if (fillImage) fillImage.color = new Color32(0xA4, 0x20, 0x20, 0xFF);
    }
}
