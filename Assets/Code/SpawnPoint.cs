using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static SpawnPoint I { get; private set; }

    [SerializeField] Transform spawnTransform;
    [SerializeField] bool restoreSanity = true;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        if (!spawnTransform) spawnTransform = transform;
    }

    public Vector3 SpawnPosition => spawnTransform ? spawnTransform.position : transform.position;
    public Quaternion SpawnRotation => spawnTransform ? spawnTransform.rotation : transform.rotation;

    public void TeleportImmediate(GameObject playerGO)
    {
        if (!playerGO) return;
        var rb = playerGO.GetComponent<Rigidbody>();
        var sanity = playerGO.GetComponent<SanitySystem>();

        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        playerGO.transform.SetPositionAndRotation(SpawnPosition, SpawnRotation);

        if (rb) rb.isKinematic = false;
        if (restoreSanity && sanity) sanity.RestoreFull();
    }
}
