using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Input : Singleton<Input>
{
    private ShooterInputActions _inputs;

    // Public events for input actions
    public event System.Action<float> OnVerticalMovement;
    public event System.Action<float> OnHorizontalRotation;
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
        _inputs.Enable();

        // Gameplay actions
        _inputs.Gameplay.VerticalMovement.performed += HandleVerticalMovement;
        _inputs.Gameplay.HorizontalRotation.performed += HandleHorizontalRotation;
        _inputs.Gameplay.Charge.started += HandleChargeStart;
        _inputs.Gameplay.Charge.canceled += HandleChargeRelease;
        _inputs.Gameplay.Pause.performed += HandlePause;
    }

    private void OnDisable()
    {
        _inputs.Disable();

        _inputs.Gameplay.VerticalMovement.performed -= HandleVerticalMovement;
        _inputs.Gameplay.HorizontalRotation.performed -= HandleHorizontalRotation;
        _inputs.Gameplay.Charge.started -= HandleChargeStart;
        _inputs.Gameplay.Charge.canceled -= HandleChargeRelease;
        _inputs.Gameplay.Pause.performed -= HandlePause;
    }

    private void HandleVerticalMovement(InputAction.CallbackContext context)
    {
        OnVerticalMovement?.Invoke(context.ReadValue<float>());
    }

    private void HandleHorizontalRotation(InputAction.CallbackContext context)
    {
        OnHorizontalRotation?.Invoke(context.ReadValue<float>());
    }

    private void HandleChargeStart(InputAction.CallbackContext context)
    {
        OnChargeStart?.Invoke();
    }

    private void HandleChargeRelease(InputAction.CallbackContext context)
    {
        OnChargeRelease?.Invoke();
    }

    private void HandlePause(InputAction.CallbackContext context)
    {
        OnPause?.Invoke();
    }
}
