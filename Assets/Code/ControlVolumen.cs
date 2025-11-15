using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class ControlVolumen : MonoBehaviour
{
    public Slider slider;
    public Image muteImage;
    public AudioMixer mixer;

    private const string PARAM = "MasterVol";
    private const string SAVE_KEY = "MasterVolValue";

    void Start()
    {
        float savedDb = PlayerPrefs.GetFloat(SAVE_KEY, -10f);
        mixer.SetFloat(PARAM, savedDb);

        slider.value = LinearFromDb(savedDb);
        UpdateMuteIcon();
    }

    public void ChangeSlider(float value)
    {
        float db = DbFromLinear(value);

        mixer.SetFloat(PARAM, db);
        PlayerPrefs.SetFloat(SAVE_KEY, db);

        UpdateMuteIcon();
    }

    private void UpdateMuteIcon()
    {
        muteImage.enabled = slider.value <= 0.001f;
    }

    private float DbFromLinear(float v)
    {
        v = Mathf.Clamp(v, 0.0001f, 1f);
        return Mathf.Log10(v) * 20f;
    }

    private float LinearFromDb(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }
}
