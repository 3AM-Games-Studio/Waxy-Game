using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public interface IInputReader {
    Vector2 Direction { get; }
    void EnablePlayerActions();
}

[CreateAssetMenu(fileName = "InputReader", menuName = "InputReader")]
public class InputReader : ScriptableObject, IPlayerActions, IInputReader {
    public event UnityAction<Vector2> Move = delegate { };
    public event UnityAction<Vector2, bool> Look = delegate { };
    public event UnityAction<bool> Jump = delegate { };
    public event UnityAction<bool> Run = delegate { };
    public event UnityAction<bool> Interact = delegate { };
    public event UnityAction<bool> Grab = delegate { };


    private PlayerInputActions inputActions;

    public bool IsJumpKeyPressed() => inputActions.Player.Jump.IsPressed();
    public Vector2 Direction => inputActions.Player.Move.ReadValue<Vector2>();
    public Vector2 LookDirection => inputActions.Player.Look.ReadValue<Vector2>();

    public bool IsUsingJoystick { get; private set; }

    bool IsDeviceMouse(InputAction.CallbackContext context) {
        // Debug.Log($"Device name: {context.control.device.name}");
        return context.control.device.name == "Mouse";
    }
    public void EnablePlayerActions() {
        if (inputActions == null) {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
        }
        inputActions.Enable();
    }

    public void DisablePlayerActions() => inputActions.Disable();
    
    private void UpdateLastUsedDevice(InputAction.CallbackContext context) {
        var device = context.control.device;
        IsUsingJoystick = device is Gamepad;
    }

    public void OnMove(InputAction.CallbackContext context) {
        if (context.performed)UpdateLastUsedDevice(context);
        Move.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context) {
        if (context.performed)UpdateLastUsedDevice(context);
        Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        switch (context.phase) {
            case InputActionPhase.Started:
                UpdateLastUsedDevice(context);
                Interact.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                UpdateLastUsedDevice(context);
                Interact.Invoke(false);
                break;
        }
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        switch (context.phase) {
            case InputActionPhase.Started:
                UpdateLastUsedDevice(context);
                Grab.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                UpdateLastUsedDevice(context);
                Grab.Invoke(false);
                break;
        }
    }


    public void OnRun(InputAction.CallbackContext context) {
        switch (context.phase) {
            case InputActionPhase.Started:
                UpdateLastUsedDevice(context);
                Run.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                UpdateLastUsedDevice(context);
                Run.Invoke(false);
                break;
        }
    }

    public void OnJump(InputAction.CallbackContext context) {
        switch (context.phase) {
            case InputActionPhase.Started:
                UpdateLastUsedDevice(context);
                Jump.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                UpdateLastUsedDevice(context);
                Jump.Invoke(false);
                break;
        }
    }

    public bool HasMovementInput() => Direction.sqrMagnitude >= 0.01f;
}
