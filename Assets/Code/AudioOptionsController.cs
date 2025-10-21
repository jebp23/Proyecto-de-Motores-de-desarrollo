using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioOptionsController : MonoBehaviour
{
    [Header("Mixer y sliders")]
    public AudioMixer mixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        // Recupera volúmenes guardados (si existen)
        float master = PlayerPrefs.GetFloat("MasterVol", 1f);
        float music = PlayerPrefs.GetFloat("MusicVol", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVol", 1f);

        masterSlider.value = master;
        musicSlider.value = music;
        sfxSlider.value = sfx;

        SetMasterVolume(master);
        SetMusicVolume(music);
        SetSFXVolume(sfx);
    }

    public void SetMasterVolume(float v)
    {
        mixer.SetFloat("MasterVol", Mathf.Lerp(-80f, 0f, v));
        PlayerPrefs.SetFloat("MasterVol", v);
    }

    public void SetMusicVolume(float v)
    {
        mixer.SetFloat("MusicVol", Mathf.Lerp(-80f, 0f, v));
        PlayerPrefs.SetFloat("MusicVol", v);
    }

    public void SetSFXVolume(float v)
    {
        mixer.SetFloat("SFXVol", Mathf.Lerp(-80f, 0f, v));
        PlayerPrefs.SetFloat("SFXVol", v);
    }
}
