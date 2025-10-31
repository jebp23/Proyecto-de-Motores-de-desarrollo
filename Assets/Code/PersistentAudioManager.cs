using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PersistentAudioManager : MonoBehaviour
{
    public static PersistentAudioManager I;

    [Header("Mixer")]
    public AudioMixer mixer;
    public AudioMixerSnapshot menuSnapshot;
    public AudioMixerSnapshot explorationSnapshot;
    public AudioMixerSnapshot chaseSnapshot;
    public AudioMixerSnapshot chaseFastSnapshot;
    public float snapshotTransitionTime = 0.5f;

    [Header("Music Sources")]
    public AudioSource musicMainMenu;
    public AudioSource musicChase;
    public AudioSource musicChaseFast;
    public AudioSource musicNoteStinger;

    [Header("Voice")]
    public AudioSource voiceBus;
    public AudioClip voNewGame;
    public AudioClip voToolFound;
    public AudioClip voVictory;
    public AudioClip voDeathGroan;

    [Header("Note Stinger Cooldown")]
    public float noteStingerCooldown = 1.5f;

    float _lastNoteTime = -999f;
    bool _isInChase = false;
    bool _isFast = false;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        EnterMenu();
    }

    void OnDestroy()
    {
        if (I == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m) { }

    public void EnterMenu()
    {
        _isInChase = false;
        _isFast = false;
        if (musicChase != null) musicChase.Stop();
        if (musicChaseFast != null) musicChaseFast.Stop();
        if (musicMainMenu != null && !musicMainMenu.isPlaying) musicMainMenu.Play();
        if (menuSnapshot != null) menuSnapshot.TransitionTo(snapshotTransitionTime);
    }

    public void EnterExploration()
    {
        _isInChase = false;
        _isFast = false;
        if (musicMainMenu != null) musicMainMenu.Stop();
        if (musicChase != null) musicChase.Stop();
        if (musicChaseFast != null) musicChaseFast.Stop();
        if (explorationSnapshot != null) explorationSnapshot.TransitionTo(snapshotTransitionTime);
    }

    public void EnterChase()
    {
        _isInChase = true;
        _isFast = false;
        if (musicMainMenu != null) musicMainMenu.Stop();
        if (musicChaseFast != null) musicChaseFast.Stop();
        if (musicChase != null && !musicChase.isPlaying) musicChase.Play();
        if (chaseSnapshot != null) chaseSnapshot.TransitionTo(snapshotTransitionTime);
    }

    public void EnterChaseFast()
    {
        _isInChase = true;
        _isFast = true;
        if (musicMainMenu != null) musicMainMenu.Stop();
        if (musicChase != null) musicChase.Stop();
        if (musicChaseFast != null && !musicChaseFast.isPlaying) musicChaseFast.Play();
        if (chaseFastSnapshot != null) chaseFastSnapshot.TransitionTo(snapshotTransitionTime);
    }

    public void OnEnemyDetected()
    {
        if (!_isFast) EnterChase();
        else EnterChaseFast();
    }

    public void OnEnemyLost()
    {
        EnterExploration();
    }

    public void OnSanityChanged01(float normalized)
    {
        if (!_isInChase) return;
        if (normalized <= 0.25f)
        {
            if (!_isFast) EnterChaseFast();
        }
        else
        {
            if (_isFast) EnterChase();
        }
    }

    public void PlayNoteStingerIfReady(AudioClip clip)
    {
        if (musicNoteStinger == null || clip == null) return;
        if (Time.unscaledTime - _lastNoteTime < noteStingerCooldown) return;
        _lastNoteTime = Time.unscaledTime;
        musicNoteStinger.clip = clip;
        musicNoteStinger.Play();
    }

    public void PlayVoice(AudioClip clip)
    {
        if (voiceBus == null || clip == null) return;
        voiceBus.Stop();
        voiceBus.clip = clip;
        voiceBus.Play();
    }

    public void StopVoice()
    {
        if (voiceBus == null) return;
        voiceBus.Stop();
    }

    public void PlayVoice_NewGame()
    {
        PlayVoice(voNewGame);
    }

    public void PlayVoice_ToolFound()
    {
        PlayVoice(voToolFound);
    }

    public void PlayVoice_Victory()
    {
        PlayVoice(voVictory);
    }

    public void PlayDeathGroan()
    {
        PlayVoice(voDeathGroan);
    }
}
