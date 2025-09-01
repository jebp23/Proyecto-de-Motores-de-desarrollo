using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _sfxSource;

    [Header("Footstep Clips")]
    [SerializeField] private AudioClip[] footstepWalkClips;
    [SerializeField] private AudioClip[] footstepRunClips;

    [Header("Monster Clips")]
    [SerializeField] private AudioClip growlClip;
    [SerializeField] private float growlCooldown = 2f; // ⏱️ tiempo mínimo entre gruñidos

    private float _lastGrowlTime;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // 🔊 Pasos
    public void PlayFootstep(bool isRunning)
    {
        AudioClip[] clips = isRunning ? footstepRunClips : footstepWalkClips;
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        _sfxSource.PlayOneShot(clip);
    }

    // 🔊 Gruñido con cooldown
    public void PlayGrowl()
    {
        if (_sfxSource != null && growlClip != null)
        {
            if (Time.time - _lastGrowlTime >= growlCooldown)
            {
                _sfxSource.PlayOneShot(growlClip);
                _lastGrowlTime = Time.time;
            }
        }
    }
}
