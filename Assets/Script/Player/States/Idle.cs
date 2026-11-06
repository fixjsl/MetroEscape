using UnityEngine;

public class Idle : PlayerState
{



    public Idle(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }
    public override void Enter()
    {
        player.Rb.linearVelocity = Vector3.zero;
        player.animator.CrossFade(PlayerAnim.idle, 0.2f);
        if (player.status.isTired)
        {
            player.animator.CrossFade(PlayerAnim.tired, 0.2f, 1);
        }
        else
            player.animator.CrossFade(PlayerAnim.idle, 0.2f, 1);
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
            if (player.status.isTired && l1.shortNameHash != PlayerAnim.tired)
                player.animator.CrossFade(PlayerAnim.tired, 0.15f, 1);
            else if (!player.status.isTired && l1.shortNameHash == PlayerAnim.tired)
            {
                if (player.equip.CurrentWeapon == null) player.animator.SetLayerWeight(1, 0f);
                else player.animator.CrossFade(PlayerAnim.idle, 0.15f, 1);
            }
        }
    }

    public override void PhysicalUpdate()
    {


    }
}
