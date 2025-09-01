using UnityEngine;

public class MonsterDetection : MonoBehaviour
{
    [Header("Sanity Damage Settings")]
    [SerializeField] private float sanityDamagePerSecond = 5f;

    private SanitySystem _sanity;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _sanity = other.GetComponent<SanitySystem>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _sanity?.TakeDamage(sanityDamagePerSecond * Time.deltaTime);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _sanity = null;
        }
    }
}
