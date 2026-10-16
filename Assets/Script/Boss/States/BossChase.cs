using UnityEngine;

namespace BossStates
{
    public class BossChase : BossState
    {
        private bool isTurning = false;

        public BossChase(BossStateMachine boss) : base(boss) { }

        public override void Enter()
        {
            Boss.animator.CrossFade(Boss.sprint, 0.01f);
            isTurning = false;
            CheckDirection();
        }

        public override void Exit() { }

        public override void LogicUpdate()
        {
            if (Boss.Targetplayer == null)
            {
                Boss.ChangeState<BossReturn>();
                return;
            }

            // 전투 범위 진입 시 즉시 공격 (BossBattle 건너뜀)
            if (Boss.IsInBattleRange())
                Boss.ChangeState<BossAttack>();
        }

        public override void PhysicalUpdate()
        {
            if (Boss.Targetplayer == null) return;

            AnimatorStateInfo stateInfo = Boss.animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != Boss.moveTurn && !Boss.animator.IsInTransition(0))
                CheckDirection();

            if (isTurning) return;

            float dirX = Boss.Targetplayer.transform.position.x - Boss.transform.position.x;
            float moveDir = dirX > 0 ? 1f : -1f;
            Boss.Rb.linearVelocity = new Vector3(moveDir * Boss.status.chasespeed, Boss.Rb.linearVelocity.y, 0f);
        }

        public override void OnTurnAnimationFinished()
        {
            float dirToPlayer = Boss.Targetplayer.transform.position.x - Boss.transform.position.x;
            float targetY = dirToPlayer > 0 ? 90f : 270f;
            Boss.Rb.rotation = Quaternion.Euler(0, targetY, 0);
            isTurning = false;
            Boss.animator.CrossFade(Boss.sprint, 0.0001f);
        }

        private void CheckDirection()
        {
            if (Boss.Targetplayer == null) return;
            float dirToPlayer = Boss.Targetplayer.transform.position.x - Boss.transform.position.x;
            float myDir = Vector3.Dot(Boss.transform.forward, Vector3.right);
            if ((dirToPlayer > 0 && myDir < 0) || (dirToPlayer < 0 && myDir > 0))
            {
                isTurning = true;
                Boss.animator.CrossFade(Boss.moveTurn, 0.01f);
            }
            else
            {
                isTurning = false;
                Boss.animator.CrossFade(Boss.sprint, 0.01f);
            }
        }
    }
}
