using UnityEngine;

namespace MonsterStates
{
    public class Stun : MonsterState
    {
        TimeManager stunTimer = new TimeManager();

        
        public Stun(MonsterStateMachine monster) : base(monster) { 

        }
        public override void Enter()
        {
            Monster.gameObject.layer = Layercache.Stun;
            Monster.Rb.linearVelocity = Vector3.zero;
            Monster.animator.speed = Monster.StunAnimLength / Mathf.Max(Monster.status.stunTime, 0.01f);
            Monster.animator.CrossFade(Monster.stun, 0.01f);
            stunTimer.Reset();
        }

        public override void Exit()
        {
            Monster.animator.speed = 1f;
            Monster.gameObject.layer = Layercache.Monster;
            Monster.status.detection_gauge = 100f;
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
