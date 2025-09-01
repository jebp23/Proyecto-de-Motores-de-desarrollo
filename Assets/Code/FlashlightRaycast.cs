using UnityEngine;

public class FlashlightRaycast : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private Light flashlight;     // La luz de la linterna
    [SerializeField] private float range = 15f;    // Alcance del raycast
    [SerializeField] private LayerMask monsterMask; // Layer para el monstruo

    [Header("Audio Settings")]
    [SerializeField] private float cooldown = 2f; // Segundos entre gruñidos
    private float _lastGrowlTime;

    private void Update()
    {
        if (flashlight == null || !flashlight.enabled) return;

        DetectMonster();
    }

    private void DetectMonster()
    {
        if (Physics.Raycast(flashlight.transform.position, flashlight.transform.forward, out RaycastHit hit, range, monsterMask))
        {
            if (hit.collider.CompareTag("Monster"))
            {
                if (Time.time >= _lastGrowlTime + cooldown)
                {
                    _lastGrowlTime = Time.time;
                    AudioManager.I?.PlayGrowl(); 
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (flashlight == null) return;

        float angle = flashlight.spotAngle * 0.5f;
        int segments = 20;
        float step = (angle * 2) / segments;


        Gizmos.color = Color.red;
        Gizmos.DrawRay(flashlight.transform.position, flashlight.transform.forward * range);

  
        Gizmos.color = Color.yellow;
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -angle + step * i;
            Quaternion rotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
            Vector3 dir = rotation * flashlight.transform.forward * range;
            Gizmos.DrawRay(flashlight.transform.position, dir);
        }
    }
}
