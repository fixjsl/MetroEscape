using UnityEngine;

public class Move : GroundMoveState
{

    protected override float walkSpeed => player.status.walkspeed;
    protected override float sprintSpeed => player.status.sprintspeed;
    protected override int moveAni => canSprint ? player.sprint : player.move;
    protected override int turnAni => canSprint? player.sprintTurn : player.moveTurn;

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

        if(curAni.shortNameHash != player.move  && !player.animator.IsInTransition(0))
        {
            player.animator.CrossFade(player.move, 0.15f);
        }
        if (player.isTired)
        {
            if(curUpperAni.shortNameHash != player.tired && !player.animator.IsInTransition(1))
                player.animator.CrossFade(player.tired, 0.15f, 1);
        }
        else if (curUpperAni.shortNameHash != player.idle && !player.animator.IsInTransition(1))
        {
            player.animator.CrossFade(player.idle, 0.15f, 1);
        }
    }


    protected override void UpdateAni()
    {
        curAni = player.animator.GetCurrentAnimatorStateInfo(0);
        curUpperAni = player.animator.GetCurrentAnimatorStateInfo(1);
        if (canSprint && curAni.shortNameHash != player.sprint)
             player.animator.CrossFade(player.sprint, 0.15f, 0);
        else if (!canSprint && curAni.shortNameHash != player.move)
             player.animator.CrossFade(player.move, 0.15f, 0);

        if (player.isTired && curUpperAni.shortNameHash != player.tired && !player.animator.IsInTransition(1))
             player.animator.CrossFade(player.tired, 0.15f, 1);
        else if (!player.isTired && curUpperAni.shortNameHash != player.idle && !player.animator.IsInTransition(1))
             player.animator.CrossFade(player.idle, 0.15f, 1);
       
    }

}
