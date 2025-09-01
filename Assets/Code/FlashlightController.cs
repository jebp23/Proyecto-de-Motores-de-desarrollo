using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private Light flashlight; 
    private PlayerInputs _input;

    private void Awake()
    {
        _input = new PlayerInputs();
    }

    private void OnEnable()
    {
        _input.Enable();
        _input.Player.Flashlight.performed += ctx => ToggleFlashlight();
    }

    private void OnDisable()
    {
        _input.Player.Flashlight.performed -= ctx => ToggleFlashlight();
        _input.Disable();
    }

    private void ToggleFlashlight()
    {
        flashlight.enabled = !flashlight.enabled;
    }
}
