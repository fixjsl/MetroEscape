using UnityEngine;

namespace BossStates
{
    public class BossAttack : BossState
    {
        public BossAttack(BossStateMachine boss) : base(boss) { }

        public override void Enter()
        {
            Boss.Rb.linearVelocity = Vector3.zero;
            Boss.Rb.AddForce(Boss.transform.forward * 20f, ForceMode.Impulse);

            if (Boss.IsFirstAttack)
            {
                // 첫 패턴: 고정된 특정 공격 애니메이션
                Boss.IsFirstAttack = false;
                Boss.animator.CrossFade(Boss.firstAttackHash, 0.01f);
            }
            else
            {
                int idx = Random.Range(0, Boss.AttackHashes.Length);
                Boss.animator.CrossFade(Boss.AttackHashes[idx], 0.01f);
            }
        }

        public override void Exit() { }

        public override void OnAnimationFinished()
        {
            Boss.IsReturningFromAttack = true;
            Boss.ChangeState<BossBattle>();
        }
    }
}
