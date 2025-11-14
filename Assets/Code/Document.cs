using UnityEngine;

public class Document : MonoBehaviour
{
    [TextArea] public string documentText;
    public bool collected = false;

    [HideInInspector] public int assignedIndex = -1;

    void OnEnable()
    {
        Debug.Log(name + " → OnEnable Frame " + Time.frameCount);
    }

    void OnDisable()
    {
        Debug.Log(name + " → OnDisable Frame " + Time.frameCount);
    }

}
