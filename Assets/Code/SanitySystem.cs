using UnityEngine;
using UnityEngine.UI;

public class SanitySystem : MonoBehaviour
{
    [SerializeField] private Slider sanitySlider;
    [SerializeField] private float maxSanity = 100f;

    private float currentSanity;

    private void Awake()
    {
        currentSanity = maxSanity;
        sanitySlider.maxValue = maxSanity;
        sanitySlider.value = currentSanity;
    }

    public void TakeDamage(float amount)
    {
        currentSanity -= amount;
        currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);
        sanitySlider.value = currentSanity;

        if (currentSanity <= 0)
        {
            Debug.Log("Perdiste chabon");
            //Recordar conectar esto con la pantalla de game over (condición de perdida)
        }
    }
}
