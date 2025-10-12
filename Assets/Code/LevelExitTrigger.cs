using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExitTrigger : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] string nextSceneName = "";
    [SerializeField] bool useBuildIndexIfEmpty = true;

    public void LoadConfiguredScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else if (useBuildIndexIfEmpty)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        LoadConfiguredScene();
    }
}
