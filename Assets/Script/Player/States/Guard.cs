using UnityEngine;

namespace PlayerStates
{
    public class Guard : PlayerState
    {



        public Guard(PlayerStateMachine stateMachine) : base(stateMachine)
        {
            isBlock = true;
        }
        public override void Enter()
        {
            //Idle Animation code
            player.animator.CrossFade(PlayerAnim.guard, 0.02f);
        }

        public override void Exit()
        {
        }

        public override void HandleDamage(float Damage)
        {

            player.status.Hp -= Damage * player.equip.CurrentWeapon.status.GuardStrength;
            player.status.UseStamina(player.equip.CurrentWeapon.status.guardStamina);
        }


    }
}
