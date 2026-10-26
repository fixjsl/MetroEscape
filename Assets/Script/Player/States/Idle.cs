using UnityEngine;

public class Idle : PlayerState
{



    public Idle(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }
    public override void Enter()
    {
        player.Rb.linearVelocity = Vector3.zero;
        player.animator.CrossFade(player.idle, 0.2f);
        if (player.isTired)
        {
            player.animator.CrossFade(player.tired, 0.2f, 1);
        }
        else
            player.animator.CrossFade(player.idle, 0.2f, 1);
    }

    public override void Exit()
    {
        //Stop Idle Animation code
        
    }



    public override void LogicUpdate()
    {
        player.status.TickStamina(false, Time.deltaTime);

        if (!player.animator.IsInTransition(1))
        {
            var l1 = player.animator.GetCurrentAnimatorStateInfo(1);
            if (player.isTired && l1.shortNameHash != player.tired)
                player.animator.CrossFade(player.tired, 0.15f, 1);
            else if (!player.isTired && l1.shortNameHash == player.tired)
            {
                if (player.currentWeapon == null) player.animator.SetLayerWeight(1, 0f);
                else player.animator.CrossFade(player.idle, 0.15f, 1);
            }
        }
    }

    public override void PhysicalUpdate()
    {


    }
}
