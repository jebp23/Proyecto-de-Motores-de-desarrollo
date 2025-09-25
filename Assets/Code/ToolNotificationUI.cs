using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ToolNotificationUI : MonoBehaviour
{
    public static ToolNotificationUI I { get; private set; }

    [SerializeField] private GameObject root;    
    [SerializeField] private float defaultSeconds = 6f;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        if (root && root.activeSelf) root.SetActive(false);
    }

    public void Show(float seconds = -1f)
    {
        if (seconds <= 0f) seconds = defaultSeconds;
        StopAllCoroutines();
        StartCoroutine(ShowCR(seconds));
    }

    private IEnumerator ShowCR(float seconds)
    {
        if (root && !root.activeSelf) root.SetActive(true);
        float t = 0f;
        while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
        if (root && root.activeSelf) root.SetActive(false);
    }
}
