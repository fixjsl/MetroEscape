using UnityEngine;

public class Attack : PlayerState
{
    private int ComboIndex = 0;
    private float lastCombo3FinishTime = float.MinValue;

    public Attack(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        canChanged = false;
    }

    // 3타 이후 쿨다운 체크 (BufferState에서 신규 공격 진입 시 사용)
    public override bool CanEnter()
    {
        if (player.currentWeapon == null) return false;
        if (player.isTired) return false;
        return Time.time - lastCombo3FinishTime >= player.currentWeapon.status.attackSpeed;
    }

    public override void Enter()
    {
        
        canChanged = false;
        player.Rb.linearVelocity = Vector3.zero;
        player.OffAttackColider();
        player.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
        player.animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
        player.animator.CrossFade(player.attackHashes[ComboIndex], 0.15f, 1);
        player.PlaySFX(player.currentWeapon.status.attackSound);
        player.status.UseStamina(player.currentWeapon.status.attackStamina);
    }

    public override void Exit()
    {
        ComboIndex = 0;
        player.CloseAllAttackWindows();
    }

    // PlayerStateMachine의 콤보 윈도우에서 호출
    public void DoCombo()
    {
        ComboIndex++;
        if(ComboIndex >2) return;
        canChanged = false;
        player.Rb.linearVelocity = Vector3.zero;
        player.animator.CrossFade(player.attackHashes[ComboIndex], 0.15f, 1);
        player.PlaySFX(player.currentWeapon.status.attackSound);
        player.status.UseStamina(player.currentWeapon.status.attackStamina);
    }

    public override void PhysicalUpdate()
    {
        player.Rb.linearVelocity = new Vector3(
            player.MoveInput * player.status.currentspeed,
            player.Rb.linearVelocity.y,
            0f);
    }

    // 애니메이션 이벤트: 캔슬/콤보 허용 구간 시작
    public override void OncanCombo()
    {
        player.OpenAttackCancelWindow(withCombo: ComboIndex < 2);
    }

    // OnAnimationFinished 기본 구현(canChanged=true)을 막음 - OnAnimationFinishedAt만 사용
    public override void OnAnimationFinished()
    {
        Debug.LogWarning($"[Attack] OnAnimationFinished 차단됨 (ComboIndex: {ComboIndex}) - 애니메이터 이벤트를 OnAnimationFinishedAt으로 교체하세요");
    }

    // 애니메이션 이벤트: 현재 공격 애니메이션 종료 (파라미터로 어떤 어택인지 식별)
    public override void OnAnimationFinishedAt(int attackIndex)
    {
        if (attackIndex != ComboIndex)
        {
            return;
        }


        if (ComboIndex >= 2)
            lastCombo3FinishTime = Time.time;

        player.OnAttackAnimFinished(ComboIndex);
        canChanged = true;
    }
}
