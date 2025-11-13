using UnityEngine;

public class RandomAmbientAudio : MonoBehaviour
{
    [SerializeField] AudioClip[] ambientClips;
    [SerializeField] float minDelay = 5f;
    [SerializeField] float maxDelay = 15f;
    [SerializeField] float minVolume = 0.8f;
    [SerializeField] float maxVolume = 1f;
    [SerializeField] bool playOnStart = true;
    [SerializeField] bool loopContinuously = true;

    AudioSource source;
    float nextPlayTime;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        if (!source) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        if (AudioManager.I != null && AudioManager.I.GetAmbienceGroup() != null)
        {
            source.outputAudioMixerGroup = AudioManager.I.GetAmbienceGroup();
        }
    }

    void Start()
    {
        if (playOnStart)
            ScheduleNextSound();
    }

    void Update()
    {
        if (!loopContinuously) return;

        if (Time.time >= nextPlayTime)
        {
            PlayRandomClip();
            ScheduleNextSound();
        }
    }

    void PlayRandomClip()
    {
        if (ambientClips == null || ambientClips.Length == 0) return;

        int index = Random.Range(0, ambientClips.Length);
        source.clip = ambientClips[index];
        source.volume = Random.Range(minVolume, maxVolume);
        source.Play();
    }

    void ScheduleNextSound()
    {
        float delay = Random.Range(minDelay, maxDelay);
        nextPlayTime = Time.time + delay;
    }
}
