using UnityEngine;

public class NoiseMeter : MonoBehaviour
{
    [Header("Noise Settings (Normalized 0–1)")]
    [SerializeField] private float decayPerSecond = 0.25f;

    private float _noise;

    public void AddNormalizedNoise(float normalizedAmount)
    {
        _noise = Mathf.Clamp01(_noise + normalizedAmount);
        GameEvents.RaiseNoiseChanged(_noise);
    }

    private void Update()
    {
        if (_noise <= 0f) return;

        _noise = Mathf.Max(0f, _noise - decayPerSecond * Time.deltaTime);
        GameEvents.RaiseNoiseChanged(_noise);
    }
       
    public float CurrentNoise01 => _noise;
}
