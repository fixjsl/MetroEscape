using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput
{
    public InputSystem_Actions Actions { get; }

    // 상태
    public float MoveInput { get; private set; }
    public bool IsSprint { get; private set; }
    public bool IsCrouch { get; private set; }      // 토글
    public bool IsGuard { get; private set; }

    // 버퍼 (지금 bufferinput / buffertime / bufferTimer)
    public StateType Buffer { get; private set; } = StateType.None;
    private const float BufferTime = 0.2f;
    private readonly IntervalTimer bufferTimer = new();

    // 판단이 필요한 키는 "눌렸다"만 알림 → PlayerStateMachine이 결정
    public event Action AttackPressed;
    public event Action GuardPressed;
    public event Action LightPressed;
    public event Action<int> QuickSlotPressed;      // 지금 QuickSlotUI에 있는 숫자 키

    private float lastMoveInput;

    public PlayerInput()
    {
        Actions = new InputSystem_Actions();
        var a = Actions.PlayerAction;
        a.Attack.performed += _ => AttackPressed?.Invoke();
        a.Dodge.performed += _ => SetBuffer(StateType.Dodge);
        a.Interact.performed += _ => SetBuffer(StateType.interact);
        a.LightTogle.performed += _ => LightPressed?.Invoke();
        a.Move.performed += ctx => { float v = ctx.ReadValue<float>(); if (lastMoveInput == 0f) { MoveInput = v; lastMoveInput = v; } };
        a.Move.canceled += _ => { MoveInput = 0f; lastMoveInput = 0f; };
        a.Crouch.performed += _ => IsCrouch = !IsCrouch;
        a.Sprint.performed += _ => IsSprint = true;
        a.Sprint.canceled += _ => IsSprint = false;
        a.Guard.performed += _ => { IsGuard = true; GuardPressed?.Invoke(); };
        a.Guard.canceled += _ => IsGuard = false;
    }

    public void Enable() => Actions.Enable();
    public void Disable() => Actions.Disable();

    public void SetBuffer(StateType t) { Buffer = t; bufferTimer.Reset(); }
    public bool ConsumeBuffer(StateType t) { if (Buffer != t) return false; Buffer = StateType.None; return true; }
    public void ClearBuffer() => Buffer = StateType.None;

    public void Tick()                                   // PlayerStateMachine.Update에서
    {
        if (Buffer != StateType.None && bufferTimer.Timer(BufferTime)) Buffer = StateType.None;
        if (Keyboard.current.digit1Key.wasPressedThisFrame) QuickSlotPressed?.Invoke(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) QuickSlotPressed?.Invoke(1);
    }
}
