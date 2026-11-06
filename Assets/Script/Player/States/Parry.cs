using System.Threading;
using UnityEngine;

namespace PlayerStates
{
    public class Parry: PlayerState
    {
        private IntervalTimer parryTimer = new IntervalTimer();

        public bool IsInParryWindow { get; private set; }

        public Parry(PlayerStateMachine stateMachine) : base(stateMachine)
        {
            isBlock = true;
        }
        public override bool CanEnter() => !player.status.isTired;

        public override void Enter()
        {
            canChanged = false;
            IsInParryWindow = true;
            parryTimer.Reset();
            player.animator.CrossFade(PlayerAnim.parrying, 0.02f);
            player.PlaySFX(player.equip.CurrentWeapon?.status.parrySound);
            player.status.UseStamina(player.equip.CurrentWeapon.status.parryStamina);
            player.combat.ParrySuccess += OnParrySuccess;
        }

        public override void Exit()
        {
            IsInParryWindow = false;
            player.combat.ParrySuccess -= OnParrySuccess;
        }

        private void OnParrySuccess()
        {

            player.animator.CrossFade(PlayerAnim.parrySuccess, 0.02f);
        }

        public override void HandleDamage(float Damage)
        {

        }

        public override void LogicUpdate()
        {
            if (parryTimer.Timer(player.status.parryduration))
            {
                IsInParryWindow = false;
                canChanged = true;
            }
        }

        public override void PhysicalUpdate()
        {


        }
    }
}
