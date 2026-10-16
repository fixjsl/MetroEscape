using UnityEngine;

public class Hit : PlayerState
{



    public Hit(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        canChanged = false;
    }
    public override void Enter()
    {
        canChanged = false;
        player.Rb.linearVelocity = Vector3.zero;
        player.animator.CrossFade(player.hit, 0.02f);
        player.PlaySFX(player.hitSound);
    }

    public override void Exit()
    {

    }

    public override void HandleDamage(float Damage)
    {
        player.status.Hp = player.status.Hp - Damage;
    }

    


    public override void LogicUpdate()
    {
        // input Logic

    }

    public override void PhysicalUpdate()
    {


    }
}