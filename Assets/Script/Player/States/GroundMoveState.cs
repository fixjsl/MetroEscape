using UnityEngine;

public abstract class GroundMoveState : PlayerState
{
    protected float movebuffer;
    protected bool canSprint;

    protected abstract float walkSpeed { get; }
    protected abstract float sprintSpeed { get; }
    protected virtual bool BeforeMove() => false;
    protected abstract int turnAni { get; }
    protected abstract int moveAni { get; }
    protected abstract void UpdateAni();
    protected void FaceDirection(float dir) => player.Rb.rotation = Quaternion.Euler(0, dir > 0 ? 90f : -90f,0);
    protected GroundMoveState(PlayerStateMachine player):base(player) { }

    public override void Enter()
    {
        float dot = Vector3.Dot(player.transform.forward, Vector3.right);
        movebuffer = (dot > 0) ? 1f : -1f;
    }
    public sealed override void LogicUpdate()
    {
        canSprint = player.isSprint && !player.isTired;
        if (BeforeMove()) return;
        if (player.MoveInput != 0 && player.MoveInput != movebuffer)
        {
            FaceDirection(movebuffer);
            canChanged = false;
            player.animator.CrossFade(turnAni, 0.15f);
            movebuffer = player.MoveInput;
        }
        if (player.animator.IsInTransition(0)) return;

        if(canChanged)UpdateAni();
        player.status.TickStamina(canSprint, Time.deltaTime);
    }
    public override void PhysicalUpdate()
    {
        if (player.isMovementLocked) return;
        if (!canChanged) return;
        player.status.currentspeed = canSprint ? sprintSpeed : walkSpeed;   
        player.Rb.linearVelocity = new Vector3(player.MoveInput * player.status.currentspeed, player.Rb.linearVelocity.y, 0f);
    }

    public override void OnTurnAnimationFinished()
    {
        FaceDirection(movebuffer);
        player.animator.CrossFade(moveAni, 0.15f);
        canChanged = true;
    }

}
