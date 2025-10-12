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

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}
