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
        player.animator.CrossFade(PlayerAnim.hit, 0.02f);
        // 공격 중에 맞으면 상체 레이어(1)가 공격 모션에 남아 있음 → 같이 idle로 돌려 놓기
        player.animator.CrossFade(PlayerAnim.idle, 0.02f, 1);
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