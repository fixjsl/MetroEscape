using UnityEngine;
using MonsterStates;

namespace BossStates
{
    public abstract class BossState : MonsterState
    {
        protected BossStateMachine Boss;

        protected BossState(BossStateMachine boss) : base(boss)
        {
            Boss = boss;
        }
    }
}
