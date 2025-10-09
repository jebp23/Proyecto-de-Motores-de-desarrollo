using UnityEngine;
using UnityEngine.UI;

public class ColdOverlayTrigger : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Image coldImage;          
    [SerializeField] Transform player;         
    [SerializeField] Collider auraTrigger;       
    [SerializeField] NoiseMeter noiseMeter;       
    [SerializeField] EnemyMonster watchedMonster;

    [Header("Fill & Alpha")]
    [SerializeField, Range(0f, 1f)] float minAlphaAtStart = 0.05f;
    [SerializeField] float fillInPerSecond = 0.40f;   
    [SerializeField] float fadeOutPerSecond = 0.60f;   

    [Header("SFX & Noise thresholds (0-1)")]
    [SerializeField, Range(0f, 1f)] float exhaleThreshold = 0.33f;
    [SerializeField, Range(0f, 1f)] float sneezeThreshold = 0.55f;

    [SerializeField] AudioSource sfxSource;     
    [SerializeField] AudioClip exhaleClip;        
    [SerializeField] AudioClip sneezeClip;         

    [SerializeField, Range(0f, 1f)] float exhaleNoise01 = 0.05f;  
    [SerializeField, Range(0f, 1f)] float sneezeNoise01 = 0.30f;  

    [SerializeField] float thresholdRearmSeconds = 2f; 
    [Header("Fade al ser descubierto")]
    [SerializeField] bool fadeWhenDetected = true;
    [SerializeField] float fadeOutOnDetectSeconds = 1.5f;

    float cold01;               
    bool playerInside;
    bool exhaleFired, sneezeFired;
    float lastExhaleTime, lastSneezeTime;
    bool wasDetecting;
    Coroutine detectFadeCo;

    void Reset()
    {
        auraTrigger = GetComponent<Collider>();
        if (auraTrigger) auraTrigger.isTrigger = true;
    }

    void Awake()
    {
        if (!noiseMeter) noiseMeter = FindFirstObjectByType<NoiseMeter>();
        if (!auraTrigger) auraTrigger = GetComponent<Collider>();
        if (auraTrigger) auraTrigger.isTrigger = true;
    }

    void Update()
    {
        float target = playerInside ? 1f : 0f;
        float rate = playerInside ? fillInPerSecond : fadeOutPerSecond;
        cold01 = Mathf.MoveTowards(cold01, target, rate * Time.deltaTime);
        ApplyVisuals(cold01);

        if (playerInside)
        {
            if (!exhaleFired && cold01 >= exhaleThreshold && Time.time - lastExhaleTime >= thresholdRearmSeconds)
            {
                Play(exhaleClip);
                if (noiseMeter) noiseMeter.AddNormalizedNoise(exhaleNoise01);
                exhaleFired = true; lastExhaleTime = Time.time;
            }

            if (!sneezeFired && cold01 >= sneezeThreshold && Time.time - lastSneezeTime >= thresholdRearmSeconds)
            {
                Play(sneezeClip);
                if (noiseMeter) noiseMeter.AddNormalizedNoise(sneezeNoise01);
                sneezeFired = true; lastSneezeTime = Time.time;
            }
        }
        else
        {
            if (cold01 < exhaleThreshold * 0.7f) exhaleFired = false;
            if (cold01 < sneezeThreshold * 0.7f) sneezeFired = false;
        }

        if (fadeWhenDetected && watchedMonster)
        {
            bool detecting = watchedMonster.CurrentlyDetecting;
            if (!wasDetecting && detecting)
            {
                if (detectFadeCo != null) StopCoroutine(detectFadeCo);
                detectFadeCo = StartCoroutine(FadeOutOnDetected());
            }
            wasDetecting = detecting;
        }
    }

    void ApplyVisuals(float t)
    {
        if (!coldImage) return;
        var c = coldImage.color;
        c.a = Mathf.Lerp(minAlphaAtStart, 1f, t);  
        coldImage.color = c;
    }

    System.Collections.IEnumerator FadeOutOnDetected()
    {
        float start = cold01;
        float dur = Mathf.Max(0.01f, fadeOutOnDetectSeconds);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            cold01 = Mathf.Lerp(start, 0f, t);
            ApplyVisuals(cold01);
            yield return null;
        }

        playerInside = false;
        exhaleFired = sneezeFired = false;
    }

    void Play(AudioClip clip)
    {
        if (!clip) return;
        if (sfxSource) sfxSource.PlayOneShot(clip);
        else AudioManager.I?.PlayOneShot(clip, 1f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = false;
    }
}
