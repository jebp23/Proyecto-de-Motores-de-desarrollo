using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip[] footstepWalkClips;
    [SerializeField] AudioClip[] footstepRunClips;
    [SerializeField, Range(0f, 1f)] float walkFootstepVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] float runFootstepVolume = 1f;
    [SerializeField] Vector2 footstepPitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] AudioClip growlClip;
    [SerializeField] float growlCooldown = 2f;

    Coroutine fadeRoutine;
    float lastGrowlTime;
    readonly Dictionary<AudioSource, float> savedVolumes = new Dictionary<AudioSource, float>();
    readonly HashSet<AudioSource> stoppedSources = new HashSet<AudioSource>();

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    public void PlayFootstep(bool running)
    {
        if (sfxSource == null) return;
        var bank = running ? footstepRunClips : footstepWalkClips;
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
        if (!clip) return;
        if (sfxSource) sfxSource.PlayOneShot(clip, volume);
        else AudioSource.PlayClipAtPoint(clip, Camera.main ? Camera.main.transform.position : Vector3.zero, volume);
    }

    public void FadeOutAll(float duration, AudioSource except = null)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeAllCo(duration, except));
    }

    public void FadeInAll(float duration)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(RestoreAllCo(duration));
    }

    public void StopAll(AudioSource except = null)
    {
        stoppedSources.Clear();
        var sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var src in sources)
        {
            if (src == null) continue;
            if (except != null && src == except) continue;
            if (src.isPlaying)
            {
                stoppedSources.Add(src);
                src.Stop();
            }
        }
    }

    public void ReplayStopped()
    {
        foreach (var src in stoppedSources)
        {
            if (src == null) continue;
            if (!src.gameObject.activeInHierarchy || !src.enabled) continue;
            if (src.clip != null) src.Play();
        }
        stoppedSources.Clear();
    }

    IEnumerator FadeAllCo(float duration, AudioSource except)
    {
        var sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        savedVolumes.Clear();
        foreach (var src in sources)
        {
            if (src == null) continue;
            if (except != null && src == except) continue;
            if (!savedVolumes.ContainsKey(src)) savedVolumes.Add(src, src.volume);
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - Mathf.Clamp01(t / Mathf.Max(0.01f, duration));
            foreach (var kv in savedVolumes)
            {
                var src = kv.Key; if (src == null) continue;
                src.volume = kv.Value * k;
            }
            yield return null;
        }

        foreach (var kv in savedVolumes)
        {
            var src = kv.Key; if (src == null) continue;
            src.volume = 0f;
        }
        fadeRoutine = null;
    }

    IEnumerator RestoreAllCo(float duration)
    {
        if (savedVolumes.Count == 0) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(0.01f, duration));
            foreach (var kv in savedVolumes)
            {
                var src = kv.Key; if (src == null) continue;
                src.volume = Mathf.Lerp(0f, kv.Value, k);
            }
            yield return null;
        }

        foreach (var kv in savedVolumes)
        {
            if (kv.Key != null) kv.Key.volume = kv.Value;
        }
        savedVolumes.Clear();
        fadeRoutine = null;
    }
}
