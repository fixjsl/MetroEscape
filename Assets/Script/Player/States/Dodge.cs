
using System.Security;
using UnityEngine;
public class Dodge : PlayerState
{
    private float cooltime;
    private float lastTime;
    
    private Vector3 dodgeVelocity;

    public Dodge(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        cooltime = player.status.DodgeCooldown;
        lastTime = -99f;
        isBlock = true;
    }

    public override bool CanEnter()
    {
        if (player.status.isTired) return false;
        return Time.time >= lastTime + cooltime && player.status.Stamina >= player.status.DodgeCost;
    }
    public override void Enter()
    {
        lastTime = Time.time;
        canChanged = false;
        player.gameObject.layer = LayerCache.Dodge;
        Physics.IgnoreLayerCollision(LayerCache.Dodge, LayerCache.Monster, true);
        player.status.UseStamina(player.status.DodgeCost);
        player.animator.SetLayerWeight(1, 0f);
        player.animator.CrossFade(PlayerAnim.dodge, 0.15f);
        player.PlaySFX(player.dodgeSound);
        dodgeVelocity = new Vector3(player.transform.forward.x * 60f, 0f, 0f);
    }

    public override void Exit()
    {
        player.gameObject.layer = LayerCache.Player;
        Physics.IgnoreLayerCollision(LayerCache.Dodge, LayerCache.Monster, false);
        player.animator.SetLayerWeight(1, 1f);
        player.Rb.linearVelocity = new Vector3(0f, player.Rb.linearVelocity.y, 0f);
    }


    public override void LogicUpdate()
    {
    }

    public override void PhysicalUpdate()
    {
        player.Rb.linearVelocity = new Vector3(dodgeVelocity.x, player.Rb.linearVelocity.y, 0f);
    }
    public override void OnAnimationFinished()
    {
        canChanged = true;
    }
}