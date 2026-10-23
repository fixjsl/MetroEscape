using MonsterStates;
using UnityEngine;

namespace MonsterStates
{
    public class Idle : MonsterState
    {
        private TimeManager Timer = new TimeManager();
        private float changeTime;
        public Idle(MonsterStateMachine monster) : base(monster)
        {
        }
        public override void Enter()
        {
            //idle애니메이션 재생
            changeTime = Random.Range(3, 5);
            Monster.Rb.linearVelocity = Vector3.zero;
            Monster.StartDetection();
            Monster.animator.CrossFade(Monster.idle, 0.01f);
            Timer.Reset();
        }
        public override void Exit()
        {

        }

        public override void LogicUpdate()
        {

            //3~5초뒤 Move로 전환
            if (Monster.Patrols && Timer.Timer(changeTime))
            {
                Monster.ChangeState<Move>();
            }
        }
        public override void PhysicalUpdate()
        {
            
        }
    }
}

