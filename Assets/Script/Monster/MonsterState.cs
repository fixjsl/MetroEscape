using UnityEngine;

namespace MonsterStates{
public abstract class MonsterState : State
{

    protected MonsterStateMachine Monster;
    protected bool HasTarget => Monster.Targetplayer != null;
    protected MonsterState(MonsterStateMachine monster)
    {
        Monster = monster;      
    }



    public override void Enter()
    {
        
    }

    public override void Exit()
    {
        
    }

    public override void HandleDamage(float Damage)
    {
            //슈퍼아머용 데미지 함수
            Monster.status.Hp -= Damage;

        }
        public virtual void OnAnimationFinished() {}
        public virtual void OnTurnAnimationFinished() { }
    }


}

