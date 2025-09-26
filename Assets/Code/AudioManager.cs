using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Output")]
    [SerializeField] AudioSource sfxSource;

    [Header("Footsteps")]
    [SerializeField] AudioClip[] footstepWalkClips;
    [SerializeField] AudioClip[] footstepRunClips;
    [SerializeField, Range(0f, 1f)] float walkFootstepVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] float runFootstepVolume = 1f;
    [SerializeField] Vector2 footstepPitchRange = new Vector2(0.95f, 1.05f);

    [Header("Misc SFX")]
    [SerializeField] AudioClip growlClip;
    [SerializeField] float growlCooldown = 2f;

    float lastGrowlTime;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    public void PlayFootstep(bool running)
    {
        if (sfxSource == null) return;
        AudioClip[] bank = running ? footstepRunClips : footstepWalkClips;
        if (bank == null || bank.Length == 0) return;
        int idx = Random.Range(0, bank.Length);
        float vol = running ? runFootstepVolume : walkFootstepVolume;
        float pitch = Random.Range(footstepPitchRange.x, footstepPitchRange.y);
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(bank[idx], vol);
    }

    public void PlayGrowl()
    {
        if (!growlClip || sfxSource == null) return;
        if (Time.time - lastGrowlTime < growlCooldown) return;
        sfxSource.PlayOneShot(growlClip);
        lastGrowlTime = Time.time;
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (!clip || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }
}
