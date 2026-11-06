using UnityEngine;

namespace PlayerStates
{
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
            if (player.equip.CurrentWeapon == null) return false;
            if (player.status.isTired) return false;
            return Time.time - lastCombo3FinishTime >= player.equip.CurrentWeapon.status.attackSpeed;
        }

        public override void Enter()
        {

            canChanged = false;
            player.Rb.linearVelocity = Vector3.zero;
            player.combat.OffAttackCollider();
            // 마지막 인자 0f: 상체 레이어가 이미 같은 공격 상태에 멈춰 있어도(공격 중 피격 등) 처음부터 다시 재생
            // → 안 그러면 OnAnimationFinishedAt이 안 와서 canChanged가 false로 남아 먹통
            player.animator.CrossFade(PlayerAnim.attackHashes[ComboIndex], 0.15f, 1, 0f);
            player.PlaySFX(player.equip.CurrentWeapon.status.attackSound);
            player.status.UseStamina(player.equip.CurrentWeapon.status.attackStamina);
        }

        public override void Exit()
        {
            ComboIndex = 0;
            player.combat.CloseAllAttackWindows();
        }

        // PlayerStateMachine의 콤보 윈도우에서 호출
        public void DoCombo()
        {
            ComboIndex++;
            if(ComboIndex >2) return;
            canChanged = false;
            player.Rb.linearVelocity = Vector3.zero;
            player.animator.CrossFade(PlayerAnim.attackHashes[ComboIndex], 0.15f, 1, 0f);
            player.PlaySFX(player.equip.CurrentWeapon.status.attackSound);
            player.status.UseStamina(player.equip.CurrentWeapon.status.attackStamina);
        }

        public override void PhysicalUpdate()
        {
            player.Rb.linearVelocity = new Vector3(
                player.input.MoveInput * player.status.currentspeed,
                player.Rb.linearVelocity.y,
                0f);
        }

        // 애니메이션 이벤트: 캔슬/콤보 허용 구간 시작
        public override void OncanCombo()
        {
            player.combat.OpenAttackCancelWindow(withCombo: ComboIndex < 2);
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

            player.combat.OnAttackAnimFinished(ComboIndex);
            canChanged = true;
        }
    }
}
