using UnityEngine;

namespace PlayerStates
{
    public class Execution : PlayerState
    {
        private MonsterStateMachine target;
        private float damage;
        public Execution(PlayerStateMachine player) : base(player)
        {
            canChanged = false;
        }

        public void setTarget(MonsterStateMachine monster)
        {
            target = monster;
        }
        public override void Enter()
        {
            canChanged = false;
            damage = player.equip.CurrentWeapon.status.attack * player.equip.CurrentWeapon.status.execution_m;
            player.animator.CrossFade(PlayerAnim.execution, 0.15f);
            player.animator.CrossFade(PlayerAnim.idle, 0.15f, 0);
            player.gameObject.layer = LayerCache.Dodge;
            player.Rb.linearVelocity = Vector3.zero;

            if (target != null)
            {
                float offset = target.status.minSeparation;
                Vector3 execPos = player.transform.position + player.transform.forward * offset;
                execPos.y = target.Rb.position.y;
                target.Rb.position = execPos;
                target.Rb.linearVelocity = Vector3.zero;
                target.Freeze();
            }
        }
        public override void Exit()
        {
            player.gameObject.layer = LayerCache.Player;
            if (target != null)
                target.Unfreeze();
        }

        public void OnDamage()
        {
            if (target != null)
            {
                target.OnExeHit(damage);
            }
        }


    }
}
