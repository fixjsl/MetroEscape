using UnityEngine;

namespace BossStates
{
    public class BossReturn : BossState
    {
        private const float arrivalThreshold = 0.5f;
        private bool isTurning;

        public BossReturn(BossStateMachine boss) : base(boss) { }

        public override void Enter()
        {
            isTurning = false;
            Boss.status.detection_gauge = 0f;
            Boss.animator.CrossFade(Boss.sprint, 0.01f);
            CheckDirection();
        }

        public override void Exit() { }

        public override void LogicUpdate()
        {
            if (Boss.spawnpoint == null) return;
            float dist = Vector3.Distance(Boss.transform.position, Boss.spawnpoint.position);
            if (dist <= arrivalThreshold)
            {
                Boss.ActivateStartTrigger(); // 스폰포인트 도착 시 전투 시작 트리거 재활성화
                Boss.ChangeState<BossIdle>();
            }
        }

        public override void PhysicalUpdate()
        {
            if (isTurning || Boss.spawnpoint == null) return;
            Vector3 dir = (Boss.spawnpoint.position - Boss.transform.position).normalized;
            Boss.Rb.linearVelocity = new Vector3(dir.x * Boss.ReturnSpeed, Boss.Rb.linearVelocity.y, 0f);
        }

        public override void OnTurnAnimationFinished()
        {
            if (Boss.spawnpoint != null)
            {
                float dirToSpawn = Boss.spawnpoint.position.x - Boss.transform.position.x;
                float targetY = dirToSpawn > 0 ? 90f : 270f;
                Boss.Rb.rotation = Quaternion.Euler(0, targetY, 0);
            }
            isTurning = false;
            Boss.animator.CrossFade(Boss.sprint, 0.0001f);
        }

        private void CheckDirection()
        {
            if (Boss.spawnpoint == null) return;
            float dirToSpawn = Boss.spawnpoint.position.x - Boss.transform.position.x;
            float myDir = Vector3.Dot(Boss.transform.forward, Vector3.right);
            if ((dirToSpawn > 0 && myDir < 0) || (dirToSpawn < 0 && myDir > 0))
            {
                isTurning = true;
                Boss.animator.CrossFade(Boss.moveTurn, 0.01f);
            }
        }
    }
}
