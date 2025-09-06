using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private Light flashlight;

    private PlayerInputs _input;
    private bool _isOn = true;

    private void Awake()
    {
        _input = new PlayerInputs();

        if (flashlight == null)
            flashlight = GetComponentInChildren<Light>();
    }

    private void OnEnable()
    {
        _input.Enable();
        _input.Player.Flashlight.performed += ToggleFlashlight;
    }

    private void OnDisable()
    {
        _input.Disable();
        _input.Player.Flashlight.performed -= ToggleFlashlight;
    }

    private void ToggleFlashlight(InputAction.CallbackContext ctx)
    {
        _isOn = !_isOn;
        flashlight.enabled = _isOn;
    }
}
