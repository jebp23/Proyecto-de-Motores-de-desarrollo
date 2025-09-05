using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private Light flashlightLight; // Ahora la referencia es solo al componente Light
    [SerializeField] private bool startOn = true;

    private PlayerInputs _input;

    private void Awake()
    {
        if (flashlightLight == null)
        {
            // Busca el componente Light en los hijos si no está asignado en el Inspector
            flashlightLight = GetComponentInChildren<Light>(true);
        }

        if (flashlightLight)
        {
            flashlightLight.enabled = startOn;
        }

        _input = new PlayerInputs();
    }

    private void OnEnable()
    {
        _input.Enable();
        _input.Player.Flashlight.started += OnFlashlightStarted;
    }

    private void OnDisable()
    {
        _input.Player.Flashlight.started -= OnFlashlightStarted;
        _input.Disable();
    }

    private void OnFlashlightStarted(InputAction.CallbackContext ctx)
    {
        if (!flashlightLight) return;
        flashlightLight.enabled = !flashlightLight.enabled;
    }
}