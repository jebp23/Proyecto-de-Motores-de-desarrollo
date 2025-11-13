using UnityEngine;

public class FlashlightRaycast : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private Light flashlight;
    [SerializeField] private float range = 15f;
    [SerializeField] private LayerMask monsterMask;

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
                // Aquí podría ir lógica futura (por ejemplo, aplicar stun o trigger visual),
                // pero ya no se reproducen sonidos.
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
