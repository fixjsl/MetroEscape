using Unity.Properties;
using UnityEngine;

namespace MonsterStates
{
    public class Hit : MonsterState
    {
        private float hitduration;
        private TimeManager Timer = new TimeManager();
        public Hit(MonsterStateMachine monster) : base(monster)
        {
        }
        public override void Enter()
        {
            Timer.Reset();
            Monster.animator.speed = Monster.HitAnimLength / Mathf.Max(hitduration, 0.01f);
            Monster.animator.CrossFade(Monster.hit, 0.01f);

            if (Monster.Targetplayer != null)
            {
                float dir = Monster.transform.position.x >= Monster.Targetplayer.transform.position.x ? 1f : -1f;
                Monster.Rb.linearVelocity = new Vector3(dir * hitduration, Monster.Rb.linearVelocity.y, 0f);
            }
        }
        public override void Exit()
        {
            Monster.animator.speed = 1f;
            Monster.Rb.linearVelocity = new Vector3(0f, Monster.Rb.linearVelocity.y, 0f);
        }

        public void SetHitduration(float duration)
        {
            hitduration = duration;
        }
        public override void LogicUpdate()
        {
            if (Timer.Timer(hitduration))
            {
                if (Monster.Targetplayer != null) Monster.ChangeState<Battle>();
                else Monster.ChangeState<Return>();
            }
        }
        public override void PhysicalUpdate()
        {
            float lerpFactor = 5f * Time.fixedDeltaTime / Mathf.Max(hitduration, 0.01f);
            Monster.Rb.linearVelocity = new Vector3(
                Mathf.Lerp(Monster.Rb.linearVelocity.x, 0f, lerpFactor),
                Monster.Rb.linearVelocity.y,
                0f);
        }
        public override void OnAnimationFinished()
        {

        }
    }

}

