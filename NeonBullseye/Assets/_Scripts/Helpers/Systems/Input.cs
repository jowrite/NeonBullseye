using UnityEngine;
using UnityEngine.InputSystem;

public class Input : Singleton<Input>
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

        //Enable the Gameplay action map by default
        _inputs.Gameplay.Enable();
    }

    private void OnEnable()
    {
        // Gameplay actions
        _inputs.Gameplay.VerticalMovement.performed += ctx => OnVerticalMovement?.Invoke(ctx.ReadValue<float>());
        _inputs.Gameplay.Rotation.performed += ctx => OnRotation?.Invoke(ctx.ReadValue<float>());
        _inputs.Gameplay.Charge.started += _ => OnChargeStart?.Invoke();
        _inputs.Gameplay.Charge.canceled += _ => OnChargeRelease?.Invoke();
        _inputs.Gameplay.Pause.performed += _ => OnPause?.Invoke();
    }

    private void OnDisable()
    {
        _inputs.Gameplay.Disable();
    }

    //Helper method to switch action maps
    public void SwitchToUIInput()
    {
        _inputs.Gameplay.Disable();
        _inputs.UI.Enable();
    }
}