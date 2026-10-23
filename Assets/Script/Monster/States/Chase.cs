using UnityEngine;


namespace MonsterStates
{
    public class Chase : MonsterState
    {
        private bool isTurning = false;
        public Chase(MonsterStateMachine monster) : base(monster)
        {
        }
        public override void Enter()
        {
            Monster.IsReturningFromAttack = false;
            Monster.StopDetection();
            Monster.animator.CrossFade(Monster.sprint, 0.01f);
            isTurning = false;
            CheckDirection();
        }
        public override void Exit()
        {
            
        }
        private void CheckDirection()
        {
            if (Monster.Targetplayer == null) return;
            isTurning = Monster.TurnToward(Monster.Targetplayer.transform.position.x, Monster.sprint);
        }
        public override void LogicUpdate()
        {
            if(Monster.Targetplayer == null)
            {
                Monster.ChangeState<Return>();
                return;
            }
            if (Monster.IsInBattleRange())
            {
                Monster.OnReachedBattleRange();
            }
        }
        public override void PhysicalUpdate()
        {
            if (Monster.Targetplayer == null) return;

            AnimatorStateInfo stateInfo = Monster.animator.GetCurrentAnimatorStateInfo(0);
            
            if (stateInfo.shortNameHash != Monster.moveTurn && !Monster.animator.IsInTransition(0))
            {
                CheckDirection();
            }
           

            if (isTurning) return;
            float dirX = Monster.Targetplayer.transform.position.x - Monster.transform.position.x;
            float moveDir = dirX > 0 ? 1f : -1f;
            Monster.Rb.linearVelocity = new Vector3(moveDir * Monster.status.chasespeed, Monster.Rb.linearVelocity.y, 0f);
            
        }
        public override void OnTurnAnimationFinished()
        {
            Monster.FaceTowardX(Monster.Targetplayer.transform.position.x);

            isTurning = false;

            Monster.animator.CrossFade(Monster.sprint, 0.0001f);
        }
    }
}

