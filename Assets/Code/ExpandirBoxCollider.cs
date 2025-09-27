using UnityEngine;

public class ExpandirBoxCollider : MonoBehaviour
{
    //Start is called once before the first execution of Update after the MonoBehaviour is created

    //CODIGO COMPLETAMENTE PROVISORIO (ARREGLAR CAMARA)Buen día gente 
    void Start()
    {
        BoxCollider[] colliders = Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None);

        foreach (BoxCollider col in colliders)
        {
            Vector3 size = col.size;
            size.x += 0.5f;
            size.y += 0.5f;
            size.z += 0.5f;
            col.size = size;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
