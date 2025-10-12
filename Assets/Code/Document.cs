using UnityEngine;

public class Document : MonoBehaviour
{
    [TextArea] public string documentText;
    public bool collected = false;

    [HideInInspector] public int assignedIndex = -1; 
}
