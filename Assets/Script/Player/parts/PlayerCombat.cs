using System;

public class PlayerCombat
{
    private readonly AttackHitbox attackHitbox;
    public PlayerCombat(AttackHitbox attackHitbox) { this.attackHitbox = attackHitbox; }
    public bool isInCancelWindow { get; private set; }
    public bool isInComboWindow { get; private set; }
    private bool postAnimWindowActive;
    private readonly TimeManager postAnimTimer = new();
    private const float PostAnimComboTime = 0.5f;
    public event Action ParrySuccess;
    public void InvokeParrySuccess() => ParrySuccess?.Invoke();
    public void OpenAttackCancelWindow(bool withCombo)
    {
        isInCancelWindow = true;
        isInComboWindow = withCombo;
    }

    public void CloseAllAttackWindows()
    {
        isInCancelWindow = false;
        isInComboWindow = false;
        postAnimWindowActive = false;
    }
    public void OnAttackAnimFinished(int comboIndex)
    {
        isInCancelWindow = false;
        if (comboIndex >= 2)
        {
            isInComboWindow = false;
            postAnimWindowActive = false;
        }
        else if (isInComboWindow)
        {
            // 1/2타 완료, 0.5초 추가 콤보 유예
            postAnimWindowActive = true;
            postAnimTimer.Reset();
        }
    }
    public void OnAttackColider() => attackHitbox.EnableHitbox();
    public void OffAttackColider() => attackHitbox.DisableHitbox();

    public void Tick()   // PlayerStateMachine.Update에서 호출 (공격 후 콤보 유예 타이머)
    {
        if (postAnimWindowActive && postAnimTimer.Timer(PostAnimComboTime))
        {
            postAnimWindowActive = false;
            isInComboWindow = false;
        }
    }
}
