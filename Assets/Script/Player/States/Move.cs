using UnityEngine;

public class Move : GroundMoveState
{

    protected override float walkSpeed => player.status.walkspeed;
    protected override float sprintSpeed => player.status.sprintspeed;
    protected override int moveAni => canSprint ? PlayerAnim.sprint : PlayerAnim.move;
    protected override int turnAni => canSprint? PlayerAnim.sprintTurn : PlayerAnim.moveTurn;

    private AnimatorStateInfo curAni;
    private AnimatorStateInfo curUpperAni;
    public Move(PlayerStateMachine stateMachine) : base(stateMachine)
    {

    }
    public override void Enter()
    {
        base.Enter();
        curAni = player.animator.GetCurrentAnimatorStateInfo(0);
        curUpperAni = player.animator.GetCurrentAnimatorStateInfo(1);
        //Move animation

        if(curAni.shortNameHash != PlayerAnim.move  && !player.animator.IsInTransition(0))
        {
            player.animator.CrossFade(PlayerAnim.move, 0.15f);
        }
        if (player.status.isTired)
        {
            if(curUpperAni.shortNameHash != PlayerAnim.tired && !player.animator.IsInTransition(1))
                player.animator.CrossFade(PlayerAnim.tired, 0.15f, 1);
        }
        else if (curUpperAni.shortNameHash != PlayerAnim.idle && !player.animator.IsInTransition(1))
        {
            player.animator.CrossFade(PlayerAnim.idle, 0.15f, 1);
        }
    }


    protected override void UpdateAni()
    {
        curAni = player.animator.GetCurrentAnimatorStateInfo(0);
        curUpperAni = player.animator.GetCurrentAnimatorStateInfo(1);
        if (canSprint && curAni.shortNameHash != PlayerAnim.sprint)
             player.animator.CrossFade(PlayerAnim.sprint, 0.15f, 0);
        else if (!canSprint && curAni.shortNameHash != PlayerAnim.move)
             player.animator.CrossFade(PlayerAnim.move, 0.15f, 0);

        if (player.status.isTired && curUpperAni.shortNameHash != PlayerAnim.tired && !player.animator.IsInTransition(1))
             player.animator.CrossFade(PlayerAnim.tired, 0.15f, 1);
        else if (!player.status.isTired && curUpperAni.shortNameHash != PlayerAnim.idle && !player.animator.IsInTransition(1))
             player.animator.CrossFade(PlayerAnim.idle, 0.15f, 1);
       
    }

}
