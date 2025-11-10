using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I;

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerSnapshot snapshotMenu;
    [SerializeField] private AudioMixerSnapshot snapshotExploration;
    [SerializeField] private AudioMixerSnapshot snapshotChase;
    [SerializeField] private AudioMixerSnapshot snapshotChaseFast;
    [SerializeField] private float snapshotTransition = 0.5f;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup voiceGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup ambienceGroup;

    [Header("Music Sources")]
    [SerializeField] private AudioSource musicMainMenu;
    [SerializeField] private AudioSource musicChase;
    [SerializeField] private AudioSource musicChaseFast;
    [SerializeField] private AudioSource musicNoteStinger;

    [Header("Voice Sources")]
    [SerializeField] private AudioSource voNewGame;
    [SerializeField] private AudioSource voToolFound;
    [SerializeField] private AudioSource voGameOver;
    [SerializeField] private AudioSource voVictory;

    [Header("SFX Sources")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource deathGroanSource;
    [SerializeField] private AudioSource detectionSfxSource;
    [SerializeField] private AudioSource stunSfxSource;
    [SerializeField] private AudioSource coldExhaleSource;
    [SerializeField] private AudioSource coldSneezeSource;
    [SerializeField] private AudioSource growlSource;

    [Header("Footstep per-scene")]
    [SerializeField] private AudioClip footstepGrass;
    [SerializeField] private AudioClip footstepWood;
    private AudioClip currentFootstepClip;

    private bool chaseDetected;
    private bool chaseFastDetected;
    private float chaseBufferEnd;
    private const float CHASE_BUFFER = 2f;
    private const float FADE_OUT = 1.5f;

    private bool voNewGamePlayed;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        AssignOutputGroups();
        SetSceneFootstepClip();
    }

    private void AssignOutputGroups()
    {
        AssignGroup(musicMainMenu, musicGroup);
        AssignGroup(musicChase, musicGroup);
        AssignGroup(musicChaseFast, musicGroup);
        AssignGroup(musicNoteStinger, musicGroup);

        AssignGroup(voNewGame, voiceGroup);
        AssignGroup(voToolFound, voiceGroup);
        AssignGroup(voGameOver, voiceGroup);
        AssignGroup(voVictory, voiceGroup);

        AssignGroup(footstepSource, sfxGroup);
        AssignGroup(deathGroanSource, sfxGroup);
        AssignGroup(detectionSfxSource, sfxGroup);
        AssignGroup(stunSfxSource, sfxGroup);
        AssignGroup(coldExhaleSource, sfxGroup);
        AssignGroup(coldSneezeSource, sfxGroup);
        AssignGroup(growlSource, sfxGroup);
    }

    private void AssignGroup(AudioSource src, AudioMixerGroup group)
    {
        if (src) src.outputAudioMixerGroup = group;
    }

    private void SetSceneFootstepClip()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        currentFootstepClip = scene.Contains("Grass") || scene.Contains("Level1") ? footstepGrass : footstepWood;
        if (footstepSource) footstepSource.clip = currentFootstepClip;
    }

    public void PlayFootstep(bool isSprinting)
    {
        if (footstepSource && currentFootstepClip)
        {
            footstepSource.pitch = isSprinting ? Random.Range(1.1f, 1.3f) : Random.Range(0.9f, 1.1f);
            footstepSource.PlayOneShot(currentFootstepClip);
        }
    }

    public void PlayDetectionSfx()
    {
        if (detectionSfxSource && detectionSfxSource.clip) detectionSfxSource.Play();
    }

    public void PlayStunSfx()
    {
        if (stunSfxSource && stunSfxSource.clip) stunSfxSource.Play();
    }

    public void PlayColdExhale()
    {
        if (coldExhaleSource && coldExhaleSource.clip) coldExhaleSource.Play();
    }

    public void PlayColdSneeze()
    {
        if (coldSneezeSource && coldSneezeSource.clip) coldSneezeSource.Play();
    }

    public void PlayDeathGroan()
    {
        if (deathGroanSource && deathGroanSource.clip) deathGroanSource.Play();
    }

    public void PlayGrowl()
    {
        if (growlSource && growlSource.clip) growlSource.Play();
    }

    public void PlayVO_NewGame()
    {
        if (voNewGamePlayed) return;
        voNewGamePlayed = true;
        if (voNewGame) voNewGame.Play();
    }

    public void PlayVO_ToolFound()
    {
        if (voToolFound) voToolFound.Play();
    }

    public void PlayVO_GameOver()
    {
        if (voGameOver) voGameOver.Play();
    }

    public void PlayVO_Victory()
    {
        if (voVictory) voVictory.Play();
    }

    public void PlayNoteStinger()
    {
        if (musicNoteStinger) musicNoteStinger.Play();
    }

    public void UpdateChaseState(bool anyMonsterDetecting, float sanityPercent)
    {
        bool wantChase = anyMonsterDetecting;
        bool wantFast = wantChase && sanityPercent <= 0.3f;

        if (wantFast) { chaseFastDetected = true; chaseDetected = false; chaseBufferEnd = Time.time + CHASE_BUFFER; }
        else if (wantChase) { chaseDetected = true; chaseFastDetected = false; chaseBufferEnd = Time.time + CHASE_BUFFER; }
        else
        {
            if (Time.time > chaseBufferEnd)
            {
                chaseDetected = false;
                chaseFastDetected = false;
            }
        }

        if (chaseFastDetected) TransitionToSnapshot(snapshotChaseFast);
        else if (chaseDetected) TransitionToSnapshot(snapshotChase);
        else TransitionToSnapshot(snapshotExploration);
    }

    private void TransitionToSnapshot(AudioMixerSnapshot target)
    {
        if (target) target.TransitionTo(snapshotTransition);
    }

    public void EnterMainMenu()
    {
        StopAllMusic();
        if (musicMainMenu) { musicMainMenu.loop = true; musicMainMenu.Play(); }
        snapshotMenu.TransitionTo(snapshotTransition);
        voNewGamePlayed = false;
    }

    public void EnterGame()
    {
        StopAllMusic();
        snapshotExploration.TransitionTo(snapshotTransition);
    }

    public void FadeOutAll(float time, AudioSource except = null)
    {
        StartCoroutine(FadeAll(time, except));
    }

    public void FadeInAll(float time)
    {
        StartCoroutine(FadeAll(time, null, true));
    }

    private IEnumerator FadeAll(float time, AudioSource except, bool fadeIn = false)
    {
        List<AudioSource> sources = new List<AudioSource>
        {
            musicMainMenu, musicChase, musicChaseFast, musicNoteStinger,
            voNewGame, voToolFound, voGameOver, voVictory,
            footstepSource, deathGroanSource, detectionSfxSource, stunSfxSource,
            coldExhaleSource, coldSneezeSource, growlSource
        };

        float start = fadeIn ? 0f : 1f;
        float end = fadeIn ? 1f : 0f;

        for (float t = 0; t < time; t += Time.unscaledDeltaTime)
        {
            float norm = t / time;
            float vol = Mathf.Lerp(start, end, norm);
            foreach (var src in sources)
            {
                if (src && src != except) src.volume = vol;
            }
            yield return null;
        }

        foreach (var src in sources)
        {
            if (src && src != except)
            {
                src.volume = end;
                if (!fadeIn) src.Stop();
            }
        }
    }

    public void StopAllMusic()
    {
        musicMainMenu?.Stop();
        musicChase?.Stop();
        musicChaseFast?.Stop();
        musicNoteStinger?.Stop();
    }
}