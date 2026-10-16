using System.Threading;
using UnityEngine;

public class Parry: PlayerState
{
    private TimeManager parryTimer = new TimeManager();

    public bool IsInParryWindow { get; private set; }

    public Parry(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        isBlock = true;
    }
    public override bool CanEnter() => !player.isTired;

    public override void Enter()
    {
        canChanged = false;
        IsInParryWindow = true;
        parryTimer.Reset();
        player.animator.CrossFade(player.parrying, 0.02f);
        player.PlaySFX(player.currentWeapon?.status.parrySound);
        player.status.UseStamina(player.currentWeapon.status.parryStamina);
        player.ParrySuccess += OnParrySuccess;
    }

    public override void Exit()
    {
        IsInParryWindow = false;
        player.ParrySuccess -= OnParrySuccess;
    }

    private void OnParrySuccess()
    {
        
        player.animator.CrossFade(player.parrysucess, 0.02f);
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