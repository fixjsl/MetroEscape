using UnityEngine;

public abstract class PlayerState : State
{
    protected PlayerStateMachine player;

    protected PlayerState(PlayerStateMachine player) 
    {
        this.player = player;
    }

    public override  void Enter()
    {
        
    }

    public override void Exit()
    {
        
    }
    public virtual void OnAnimationFinished()
    {
        canChanged = true;
    }
    public virtual void OnTurnAnimationFinished() { }
    public virtual void OncanCombo() { }
    public virtual void OnAnimationFinishedAt(int attackIndex) { }

}
