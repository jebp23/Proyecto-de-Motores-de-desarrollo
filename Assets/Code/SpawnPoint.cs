using System.Collections;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static SpawnPoint I { get; private set; }

    [Header("Settings")]
    [SerializeField] private Transform spawnTransform;   // si lo dejas vac�o, usa el propio transform
    [SerializeField] private float respawnDelay = 0.25f;  // peque�a espera para UI/FX
    [SerializeField] private bool restoreSanity = true;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        if (spawnTransform == null) spawnTransform = transform;
    }

    /// <summary>
    /// Reubica al jugador en el spawn. Resetea velocidad y, opcionalmente, la cordura.
    /// </summary>
    public void RespawnPlayer(GameObject playerGO)
    {
        if (playerGO == null) return;
        StartCoroutine(RespawnCR(playerGO));
    }

    private IEnumerator RespawnCR(GameObject playerGO)
    {
        yield return new WaitForSeconds(respawnDelay);

        var rb = playerGO.GetComponent<Rigidbody>();
        var ctrl = playerGO.GetComponent<MonoBehaviour>(); // tu controller (si quer�s desactivar mientras mueve)
        var sanity = playerGO.GetComponent<SanitySystem>();

        // (opcional) desactivar tu controller de movimiento mientras teletransporta
        // if (ctrl) ctrl.enabled = false;

        if (rb)
        {
            // reset de velocidades antes de mover
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero; // Unity 6 expone esta propiedad
#endif
            rb.isKinematic = true; // evita �saltos� al setear posici�n/rotaci�n
        }

        playerGO.transform.SetPositionAndRotation(spawnTransform.position, spawnTransform.rotation);

        if (rb) rb.isKinematic = false;
        if (restoreSanity && sanity != null) sanity.RestoreFull();

        // if (ctrl) ctrl.enabled = true;
    }
}
