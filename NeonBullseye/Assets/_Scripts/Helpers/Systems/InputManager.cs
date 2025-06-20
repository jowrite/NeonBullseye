using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : Singleton<InputManager>
{
    private ShooterInputActions _inputs;

    // Public events for input actions
    public event System.Action<float> OnVerticalMovement;
    public event System.Action<float> OnRotation;
    public event System.Action OnChargeStart;
    public event System.Action OnChargeRelease;
    public event System.Action OnPause;

    protected override void Awake()
    {
        base.Awake();
        _inputs = new ShooterInputActions();
    }

    private void OnEnable()
    {
        //Enable the Gameplay action map by default
        _inputs.Gameplay.Enable();

        // Gameplay actions
        _inputs.Gameplay.VerticalMovement.performed += HandleVerticalMovement;
        _inputs.Gameplay.Rotation.performed += HandleRotation;
        _inputs.Gameplay.Charge.started += HandleChargeStart;
        _inputs.Gameplay.Charge.canceled += HandleChargeRelease;
        _inputs.Gameplay.Pause.performed += HandlePause;
    }

    private void OnDisable()
    {
        _inputs.Gameplay.Disable();

        _inputs.Gameplay.VerticalMovement.performed -= HandleVerticalMovement;
        _inputs.Gameplay.Rotation.performed -= HandleRotation;
        _inputs.Gameplay.Charge.started -= HandleChargeStart;
        _inputs.Gameplay.Charge.canceled -= HandleChargeRelease;
        _inputs.Gameplay.Pause.performed -= HandlePause;

    }

    //Helper method to switch action maps
    public void SwitchToUIInput()
    {
        _inputs.Gameplay.Disable();
        _inputs.UI.Enable();
    }

    private void HandleVerticalMovement(InputAction.CallbackContext ctx)
    {
        OnVerticalMovement?.Invoke(ctx.ReadValue<float>());
    }

    private void HandleRotation(InputAction.CallbackContext ctx)
    {
        OnRotation?.Invoke(ctx.ReadValue<float>());
    }

    private void HandleChargeStart(InputAction.CallbackContext ctx)
    {
        OnChargeStart?.Invoke();
    }

    private void HandleChargeRelease(InputAction.CallbackContext ctx)
    {
        OnChargeRelease?.Invoke();
    }

    private void HandlePause(InputAction.CallbackContext ctx)
    {
        OnPause?.Invoke();
    }

}