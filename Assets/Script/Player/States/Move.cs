using UnityEngine;

public class Move : PlayerState
{

    private float movebuffer;
    bool canSprint;
    private AnimatorStateInfo curAni;
    private AnimatorStateInfo curUpperAni;
    public Move(PlayerStateMachine stateMachine) : base(stateMachine)
    {

    }
    public override void Enter()
    {
        curAni = player.animator.GetCurrentAnimatorStateInfo(0);
        curUpperAni = player.animator.GetCurrentAnimatorStateInfo(1);
        //Move animation
        float dot = Vector3.Dot(player.transform.forward, Vector3.right);
        movebuffer = (dot > 0) ? 1f : -1f;
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

    public override void Exit()
    {
    }
    public override void LogicUpdate()
    {
        canSprint = player.isSprint && !player.isTired;
        if (player.MoveInput != 0 && player.MoveInput != movebuffer)
        {
            float targetY = (movebuffer > 0) ? 90f : -90f;
            player.Rb.rotation = Quaternion.Euler(0, targetY, 0);
            canChanged = false;
            if (canSprint)
            {
                player.animator.CrossFade(player.sprintTurn, 0.15f);
            }
            else
            {
                player.animator.CrossFade(player.moveTurn, 0.15f);
            }
            movebuffer = player.MoveInput;
        }
        if(player.animator.IsInTransition(0)) return;
        curAni = player.animator.GetCurrentAnimatorStateInfo(0);
        curUpperAni = player.animator.GetCurrentAnimatorStateInfo(1);
        if (canChanged)
        {
            if (canSprint && curAni.shortNameHash != player.sprint)
                player.animator.CrossFade(player.sprint, 0.15f,0);
            else if (!canSprint && curAni.shortNameHash != player.move)
                player.animator.CrossFade(player.move, 0.15f,0);

            if (player.isTired && curUpperAni.shortNameHash != player.tired && !player.animator.IsInTransition(1))
                player.animator.CrossFade(player.tired, 0.15f, 1);
            else if (!player.isTired && curUpperAni.shortNameHash != player.idle && !player.animator.IsInTransition(1))
                player.animator.CrossFade(player.idle, 0.15f, 1);
        }

        if (!player.isSprint || player.isTired)
        {
            player.status.Stamina += player.status.StaminaRecoveryRate * Time.deltaTime;
        }
        else
        {
            player.status.Stamina -= player.status.SprintCost * Time.deltaTime;
        }
    }

    public override void PhysicalUpdate()
    {
        if (player.isMovementLocked) return;

        if (canChanged)
        {
            
            if (player.status.currentspeed != player.status.walkspeed)
                player.status.currentspeed = player.status.walkspeed;
            if (canSprint && player.status.currentspeed != player.status.sprintspeed)
                player.status.currentspeed = player.status.sprintspeed;

            player.Rb.linearVelocity = new Vector3(
                player.MoveInput * player.status.currentspeed,
                player.Rb.linearVelocity.y,
                0f);
        }

    }
    public override void OnTurnAnimationFinished()
    {
        
        float targetY = (movebuffer > 0) ? 90f : -90f;
        player.Rb.rotation = Quaternion.Euler(0, targetY, 0);
        if (canSprint) 
        {
            player.animator.CrossFade(player.sprint, 0.15f);
        }
        else
        {
            player.animator.CrossFade(player.move, 0.15f);
        }
        canChanged = true;
    }
}
