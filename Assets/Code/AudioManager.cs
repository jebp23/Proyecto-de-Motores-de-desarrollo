using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Footsteps")]
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioClip _footstepClip;
    [SerializeField] private float _baseFootstepIntervalAtWalk = 0.44f;


    public float BaseFootstepIntervalAtWalk => _baseFootstepIntervalAtWalk;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    public void PlayFootstep(bool isRunning)
    {
        if (_footstepClip == null) return;

        _sfxSource.pitch = isRunning
            ? Random.Range(1.0f, 1.2f)
            : Random.Range(0.8f, 1.0f);

        _sfxSource.PlayOneShot(_footstepClip);
    }
}

