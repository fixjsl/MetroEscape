using UnityEngine;

namespace BossStates
{
    public class BossStun : BossState
    {
        private TimeManager stunTimer = new TimeManager();

        public BossStun(BossStateMachine boss) : base(boss) { }

        public override void Enter()
        {
            Boss.gameObject.layer = Layercache.Stun;
            Boss.Rb.linearVelocity = Vector3.zero;
            Boss.animator.CrossFade(Boss.stun, 0.01f);
            stunTimer.Reset();
        }

        public override void Exit()
        {
            Boss.gameObject.layer = Layercache.Monster;
        }

        public override void LogicUpdate()
        {
            if (stunTimer.Timer(Boss.status.stunTime))
            {
                if (Boss.Targetplayer != null) Boss.ChangeState<BossBattle>();
                else Boss.ChangeState<BossReturn>();
            }
        }

        // 스턴 중에도 데미지는 받음
        public override void HandleDamage(float Damage)
        {
            Boss.status.Hp -= Damage;
        }
    }
}
