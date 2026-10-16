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

            // �÷��̾ ���������� ��������
            float dirToPlayer = Monster.Targetplayer.transform.position.x - Monster.transform.position.x;
            // ���� �ٶ󺸴� ����
            float myDir = Vector3.Dot(Monster.transform.forward, Vector3.right);

            // �ݴ� �����̸� Turn
            if ((dirToPlayer > 0 && myDir < 0) || (dirToPlayer < 0 && myDir > 0))
            {
                isTurning = true;
                Monster.animator.CrossFade(Monster.moveTurn, 0.01f);
            }
            else
            {
                isTurning = false;
                Monster.animator.CrossFade(Monster.sprint, 0.01f);
            }
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
                Monster.ChangeState<Battle>();
            }
        }
        public override void PhysicalUpdate()
        {
            //��ǥ���� �̵�
            if (Monster.Targetplayer == null) return;

            AnimatorStateInfo stateInfo = Monster.animator.GetCurrentAnimatorStateInfo(0);
            // �÷��̾� �������� ȸ��
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
            float dirToPlayer = Monster.Targetplayer.transform.position.x - Monster.transform.position.x;
            float targetY = dirToPlayer > 0 ? 90f : 270f;
            Monster.Rb.rotation = Quaternion.Euler(0, targetY, 0);

            isTurning = false;

            Monster.animator.CrossFade(Monster.sprint, 0.0001f);
        }
    }
}

