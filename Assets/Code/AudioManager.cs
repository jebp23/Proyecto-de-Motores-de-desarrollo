using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

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
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float escapeGraceSeconds = 3f;

    [Header("Voice Sources")]
    [SerializeField] private AudioSource voNewGame;
    [SerializeField] private AudioSource voToolFound;
    [SerializeField] public AudioSource voGameOver;
    [SerializeField] private AudioSource voVictory;

    [Header("SFX Sources")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource deathGroanSource;
    [SerializeField] private AudioSource coldExhaleSource;
    [SerializeField] private AudioSource coldSneezeSource;

    [Header("Footstep per-scene")]
    [SerializeField] private AudioClip footstepGrass;
    [SerializeField] private AudioClip footstepWood;
    [SerializeField] private string level1Name = "Level1";
    [SerializeField] private string level2Name = "Level2";
    private AudioClip currentFootstepClip;

    private Coroutine fadeCoroutine;
    private bool voNewGamePlayed;
    private bool lastChaseState;
    private bool lastFastState;
    private bool isChasingMusicActive;
    public AudioMixerSnapshot SnapshotExploration => snapshotExploration;



    void Awake()
    {
        Debug.Log("AudioManager → Awake()");
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        AssignOutputGroups();
        SetSceneFootstepClip();
    }

    void Start() 
    {
        Debug.Log("AudioManager → Start()");
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnImportantNoteCollected()
    {
        if (musicNoteStinger && !musicNoteStinger.isPlaying)
            musicNoteStinger.Play();
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
        AssignGroup(coldExhaleSource, sfxGroup);
        AssignGroup(coldSneezeSource, sfxGroup);
    }

    private void AssignGroup(AudioSource src, AudioMixerGroup group)
    {
        if (src) src.outputAudioMixerGroup = group;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => SetSceneFootstepClip();

    private void SetSceneFootstepClip()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == level1Name) currentFootstepClip = footstepGrass;
        else if (scene == level2Name) currentFootstepClip = footstepWood;
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

    public void PlayColdExhale() { if (coldExhaleSource && coldExhaleSource.clip) coldExhaleSource.Play(); }
    public void PlayColdSneeze() { if (coldSneezeSource && coldSneezeSource.clip) coldSneezeSource.Play(); }
    public void PlayDeathGroan() { if (deathGroanSource && deathGroanSource.clip) deathGroanSource.Play(); }

    public void PlayVO_NewGame()
    {
        if (voNewGamePlayed) return;
        voNewGamePlayed = true;
        if (voNewGame) voNewGame.Play();
    }

    public void PlayVO_ToolFound() { if (voToolFound) voToolFound.Play(); }
    public void PlayVO_GameOver() { if (voGameOver) voGameOver.Play(); }
    public void PlayVO_Victory() { if (voVictory) voVictory.Play(); }
    public void PlayNoteStinger()
    {
        Debug.Log("STINGER → PlayNoteStinger() llamado de: " + Time.frameCount);
        if (musicNoteStinger) musicNoteStinger.Play();
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

    public void StopAllMusic()
    {
        musicMainMenu?.Stop();
        musicChase?.Stop();
        musicChaseFast?.Stop();
        musicNoteStinger?.Stop();
    }

    public void UpdateChaseState(bool isChasing, float sanityPercent)
    {
        bool isFast = sanityPercent <= 0.3f;

        if (isChasing && !lastChaseState)
        {
            PlayChaseMusic(isFast);
        }
        else if (!isChasing && lastChaseState)
        {
            StopChaseMusic();
        }

        if (isChasing && isFast != lastFastState)
        {
            PlayChaseMusic(isFast);
        }

        if (isChasing)
        {
            if (isFast) snapshotChaseFast.TransitionTo(snapshotTransition);
            else snapshotChase.TransitionTo(snapshotTransition);
        }
        else snapshotExploration.TransitionTo(snapshotTransition);

        lastChaseState = isChasing;
        lastFastState = isFast;
    }

    public void PlayChaseMusic(bool isFast)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        StopAllMusic();

        if (isFast && musicChaseFast != null)
        {
            musicChaseFast.volume = 1f;
            musicChaseFast.loop = true;
            musicChaseFast.Play();
        }
        else if (musicChase != null)
        {
            musicChase.volume = 1f;
            musicChase.loop = true;
            musicChase.Play();
        }

        isChasingMusicActive = true;
    }

    public void StopChaseMusic()
    {
        if (!isChasingMusicActive) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutChaseMusic());
    }

    private IEnumerator FadeOutChaseMusic()
    {
        yield return new WaitForSeconds(escapeGraceSeconds);

        AudioSource activeSource = null;
        if (musicChase != null && musicChase.isPlaying) activeSource = musicChase;
        else if (musicChaseFast != null && musicChaseFast.isPlaying) activeSource = musicChaseFast;

        if (activeSource != null)
        {
            float startVol = activeSource.volume;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                activeSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
                yield return null;
            }
            activeSource.Stop();
            activeSource.volume = 1f;
        }

        isChasingMusicActive = false;
    }

    public void FadeOutAll(float time, AudioSource except = null)
    {
        StartCoroutine(FadeAll(time, except, false));
    }

    public void FadeInAll(float time)
    {
        StartCoroutine(FadeAll(time, null, true));
    }

    private IEnumerator FadeAll(float time, AudioSource except, bool fadeIn)
    {
        List<AudioSource> sources = new List<AudioSource>
        {
            musicMainMenu, musicChase, musicChaseFast, musicNoteStinger,
            voNewGame, voToolFound, voGameOver, voVictory,
            footstepSource, deathGroanSource, coldExhaleSource, coldSneezeSource
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
                if (!fadeIn && src.isPlaying) src.Stop();
            }
        }
    }

    public void ForceStopChaseMusic()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (musicChase != null && musicChase.isPlaying)
        {
            musicChase.Stop();
            musicChase.volume = 1f;
        }

        if (musicChaseFast != null && musicChaseFast.isPlaying)
        {
            musicChaseFast.Stop();
            musicChaseFast.volume = 1f;
        }

        isChasingMusicActive = false;
        lastChaseState = false;
        lastFastState = false;
    }

    public void StopAllMusicNow()
    {
        if (musicMainMenu) musicMainMenu.Stop();
        if (musicChase) musicChase.Stop();
        if (musicChaseFast) musicChaseFast.Stop();
        if (musicNoteStinger) musicNoteStinger.Stop();
    }

    public UnityEngine.Audio.AudioMixerGroup GetAmbienceGroup()
    {
        return ambienceGroup;
    }


}

