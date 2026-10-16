using UnityEngine;

namespace BossStates
{
    public class BossIdle : BossState
    {
        public BossIdle(BossStateMachine boss) : base(boss) { }

        public override void Enter()
        {
            Boss.Rb.linearVelocity = Vector3.zero;
            Boss.animator.CrossFade(Boss.idle, 0.1f);
        }

        public override void Exit() { }
    }
}
