using UnityEngine;

namespace BossStates
{
    public class BossHit : BossState
    {
        private float hitDuration;
        private TimeManager timer = new TimeManager();

        public BossHit(BossStateMachine boss) : base(boss) { }

        public void SetHitDuration(float duration) => hitDuration = duration;

        public override void Enter()
        {
            timer.Reset();
            Boss.animator.speed = Boss.HitAnimLength / Mathf.Max(hitDuration, 0.01f);
            Boss.animator.CrossFade(Boss.hit, 0.01f);

            if (Boss.Targetplayer != null)
            {
                float dir = Boss.transform.position.x >= Boss.Targetplayer.transform.position.x ? 1f : -1f;
                Boss.Rb.linearVelocity = new Vector3(dir * hitDuration, Boss.Rb.linearVelocity.y, 0f);
            }
        }

        public override void Exit()
        {
            Boss.animator.speed = 1f;
            Boss.Rb.linearVelocity = new Vector3(0f, Boss.Rb.linearVelocity.y, 0f);
        }

        public override void LogicUpdate()
        {
            if (timer.Timer(hitDuration))
            {
                if (Boss.Targetplayer != null)
                {
                    // BossBattle 복귀 시 타이머 유지 (다음 공격까지 남은 시간 보존)
                    Boss.PreserveBattleTimer = true;
                    Boss.ChangeState<BossBattle>();
                }
                else
                {
                    Boss.ChangeState<BossReturn>();
                }
            }
        }

        public override void PhysicalUpdate()
        {
            float lerpFactor = 5f * Time.fixedDeltaTime / Mathf.Max(hitDuration, 0.01f);
            Boss.Rb.linearVelocity = new Vector3(
                Mathf.Lerp(Boss.Rb.linearVelocity.x, 0f, lerpFactor),
                Boss.Rb.linearVelocity.y,
                0f);
        }
    }
}
