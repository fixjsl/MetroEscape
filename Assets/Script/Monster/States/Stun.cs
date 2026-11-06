using UnityEngine;

namespace MonsterStates
{
    public class Stun : MonsterState
    {
        IntervalTimer stunTimer = new IntervalTimer();

        
        public Stun(MonsterStateMachine monster) : base(monster) { 

        }
        public override void Enter()
        {
            Monster.gameObject.layer = LayerCache.Stun;
            Monster.Rb.linearVelocity = Vector3.zero;
            if (Monster.ScalesStunAnim)
                Monster.animator.speed = Monster.StunAnimLength / Mathf.Max(Monster.status.stunTime, 0.01f);
            Monster.animator.CrossFade(MonsterAnim.stun, 0.01f);
            stunTimer.Reset();
        }

        public override void Exit()
        {
            Monster.animator.speed = 1f;
            Monster.gameObject.layer = LayerCache.Monster;
            Monster.status.detection_gauge = MonsterStateMachine.MaxDetectionGauge;
        }
        public override void LogicUpdate()
        {
            if (stunTimer.Timer(Monster.status.stunTime))
            {
                if (Monster.Targetplayer == null)
                {
                    Monster.ChangeState<Return>();
                }
                else  Monster.ChangeState<Battle>();
            }
        }


    }
}
