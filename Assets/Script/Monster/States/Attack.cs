using UnityEngine;

namespace MonsterStates
{
    public class Attack : MonsterState
    {
        public Attack(MonsterStateMachine monster) : base(monster)
        {
        }
        public override void Enter()
        {
            Monster.Rb.linearVelocity = Vector3.zero;
            Monster.Rb.AddForce(Monster.transform.forward * 20f, ForceMode.Impulse);
            int randomIndex = Random.Range(0, Monster.AttackHashes.Length);
            Monster.animator.CrossFade(Monster.AttackHashes[randomIndex], 0.01f);
        }
        public override void Exit()
        {
            
        }

        public override void LogicUpdate()
        {

        }
        public override void PhysicalUpdate()
        {

        }
        public override void OnAnimationFinished()
        {
            Monster.IsReturningFromAttack = true;
            Monster.ChangeState<Battle>();
        }

    }

    }

