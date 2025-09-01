using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private Light flashlight;
    [SerializeField] private bool startOn = true;

    private PlayerInputs _input;

    private void Awake()
    {
        if (!flashlight) flashlight = GetComponentInChildren<Light>(true);
        if (flashlight) flashlight.enabled = startOn;

        _input = new PlayerInputs();   // clase generada desde tu .inputactions
    }

    private void OnEnable()
    {
        _input.Enable();
        // IMPORTANTE: para toggles usa 'started' o configura la acción con "Press (Press Only)"
        _input.Player.Flashlight.started += OnFlashlightStarted;
    }

    private void OnDisable()
    {
        _input.Player.Flashlight.started -= OnFlashlightStarted;
        _input.Disable();
    }

    private void OnFlashlightStarted(InputAction.CallbackContext ctx)
    {
        if (!flashlight) return;
        flashlight.enabled = !flashlight.enabled;
    }
}
