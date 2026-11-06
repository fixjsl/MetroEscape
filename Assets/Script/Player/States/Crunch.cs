
using UnityEngine;

public class Crunch : GroundMoveState
{


    protected override float walkSpeed => player.status.crunchspeed;
    protected override float sprintSpeed => player.status.crunchsprintspeed;
    protected override int moveAni => PlayerAnim.crunchmove;
    protected override int turnAni => PlayerAnim.crunchturn;
    private AnimatorStateInfo curAni;
    private AnimatorStateInfo curUpperAni;
    public Crunch(PlayerStateMachine stateMachine) : base(stateMachine)
    {

    }
    public override void Enter()
    {
        base.Enter();

        player.animator.CrossFade(PlayerAnim.crunch, 0.15f);
        if (player.status.isTired && !player.animator.IsInTransition(1))
            player.animator.CrossFade(PlayerAnim.tired, 0.15f, 1);

        player.Col.size = new Vector3(player.Col.size.x, player.crouchHeight, player.Col.size.z);
        player.Col.center = new Vector3(player.Col.center.x, player.crouchHeight / 2f, player.Col.center.z);
    }

    public override void Exit()
    {
        player.Col.size = player.StandSize;
        player.Col.center = player.StandCenter;
    }
    protected override bool BeforeMove()
    {
        curAni = player.animator.GetCurrentAnimatorStateInfo(0);
        canSprint = player.input.IsSprint && !player.status.isTired;
        if (!player.input.IsCrouch)
        {
            if (curAni.shortNameHash != PlayerAnim.outcrunch && !player.animator.IsInTransition(0))
            {
                canChanged = false;
                player.animator.CrossFade(PlayerAnim.outcrunch, 0.15f);
            }
            return true;
        }
        return false;
    }
    protected override void UpdateAni()
    {
    
        curUpperAni = player.animator.GetCurrentAnimatorStateInfo(1);
            if(player.input.MoveInput != 0)
            {
                if (curAni.shortNameHash != PlayerAnim.crunchmove)
                    player.animator.CrossFade(PlayerAnim.crunchmove, 0.15f, 0);
            }
            else if (curAni.shortNameHash != PlayerAnim.crunch)
                player.animator.CrossFade(PlayerAnim.crunch, 0.15f, 0);

            if (player.status.isTired && curUpperAni.shortNameHash != PlayerAnim.tired && !player.animator.IsInTransition(1))
                player.animator.CrossFade(PlayerAnim.tired, 0.15f, 1);

    }


}
