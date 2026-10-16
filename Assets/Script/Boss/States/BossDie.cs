using UnityEngine;

namespace BossStates
{
    public class BossDie : BossState
    {
        public BossDie(BossStateMachine boss) : base(boss) { }

        public override void Enter()
        {
            Boss.animator.CrossFade(Boss.die, 0.001f);
            Boss.Rb.linearVelocity = Vector3.zero;
            Boss.enabled = false;
            if (Boss.AttackCollider != null) Boss.AttackCollider.enabled = false;
            var col = Boss.GetComponent<Collider>();
            col.isTrigger = true;
            col.enabled = true;
            Boss.gameObject.layer = Layercache.Die;
            if (Boss.Corpse != null) Boss.Corpse.enabled = true;
        }

        public override void Exit()
        {
            Boss.status.Hp = Boss.status.Maxhp;
        }
    }
}
