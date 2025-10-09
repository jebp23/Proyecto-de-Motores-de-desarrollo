using UnityEngine;

[DisallowMultipleComponent]
public class EnemyColdAura : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("UI Controller")]
    [SerializeField] private MonoBehaviour uiBehaviour; // arrastrá aquí tu ColdFourSidesUI
    IColdUI ui;                                         // se resuelve en runtime

    [Header("Falloff")]
    [SerializeField] private float maxRadius = 18f;
    [SerializeField] private float innerRadius = 4.5f;
    [SerializeField] private AnimationCurve falloff = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Smoothing")]
    [SerializeField] private float sendEvery = 0.05f;
    float timer;

    void Start()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // Intentamos por inspector; si no, buscamos en escena
        ui = uiBehaviour as IColdUI;
        if (ui == null)
        {
            var four = FindFirstObjectByType<ColdFourSidesUI>(FindObjectsInactive.Include);
            if (four) ui = four as IColdUI;
            // (si en algún momento usás el radial, lo podrías resolver aquí también)
        }
    }

    void Update()
    {
        if (!player || ui == null) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = sendEvery;

        float d = Vector3.Distance(transform.position, player.position);
        float t;
        if (d <= innerRadius) t = 1f;
        else if (d >= maxRadius) t = 0f;
        else
        {
            float k = Mathf.InverseLerp(maxRadius, innerRadius, d); // 0 lejos → 1 cerca
            t = Mathf.Clamp01(falloff.Evaluate(k));
        }

        ui.SetTarget01(t);
    }
}
