using UnityEngine;

public class TorsoAimToCamera : MonoBehaviour
{
    public Transform followCamera;     
    public Transform playerRoot;       
    public float distance = 2f;        
    public float heightOffset = 0.5f;  
    public float smoothSpeed = 10f;   

    void LateUpdate()
    {
        if (!followCamera || !playerRoot) return;

        Vector3 desiredPos = playerRoot.position + followCamera.transform.forward * distance;
        desiredPos.y += heightOffset;
               
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);
      
        Vector3 lookDir = followCamera.forward;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * smoothSpeed);
    }
}
