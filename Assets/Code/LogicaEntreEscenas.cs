using UnityEngine;

public class LogicaEntreEscenas : MonoBehaviour
{
    private void Awake()
    {
        var dontDestroyBetweenScenes = FindObjectsOfType<LogicaEntreEscenas>();
        if (dontDestroyBetweenScenes.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
