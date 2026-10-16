using UnityEngine;

namespace MonsterStates
{
    public class Move : MonsterState
    {
        private TimeManager Timer = new TimeManager();
        private float changeTime;
        private float MoveDir;
        public Move(MonsterStateMachine monster) : base(monster)
        {
            
        }
        public override void Enter()
        {
            changeTime = Random.Range(2f, 4f);
            float dot = Vector3.Dot(Monster.transform.forward, Vector3.right);
            MoveDir = (dot > 0) ? 1f : -1f;
            Monster.animator.CrossFade(Monster.moveTurn, 0.01f);
            canChanged = false;
            Timer.Reset();
        }
        public override void Exit()
        {

        }

        public override void LogicUpdate()
        {
            if (Timer.Timer(changeTime))
            {
                Monster.ChangeState<Idle>();
            }
        }
        public override void PhysicalUpdate()
        {
            if (canChanged)
            {
                Monster.Rb.linearVelocity = new Vector3(MoveDir * Monster.status.speed, Monster.Rb.linearVelocity.y, 0f);
            }
        }

        public override void OnTurnAnimationFinished()
        {
            // 현재 방향 반대로 180도 회전
            MoveDir = -MoveDir;
            float targetY = (MoveDir > 0) ? 90f : -90f;
            Monster.Rb.rotation = Quaternion.Euler(0, targetY, 0);

            canChanged = true;
            Monster.animator.CrossFade(Monster.move, 0.0001f);
        }
    }
}

