using UnityEngine;

namespace BossStates
{
    // 패링 불가 특수 공격
    // isBlock = true 로 설정해 모션 중 피격/스턴 무효
    public class BossSpecial : BossState
    {
        public BossSpecial(BossStateMachine boss) : base(boss) { }

        public override void Enter()
        {
            isBlock = true;
            Boss.ResetSpecialCooldown();
            Boss.Rb.linearVelocity = Vector3.zero;
            Boss.animator.CrossFade(Boss.specialHash, 0.01f);
        }

        public override void Exit()
        {
            isBlock = false;
        }

        public override void OnAnimationFinished()
        {
            Boss.ChangeState<BossBattle>();
        }
    }
}
