using UnityEngine;


public abstract class  State : IState
{

    public bool canChanged { get; protected set; } = true;
    public bool isBlock = false;


    protected State()
    {
       
    }

    public abstract void Enter();

    public virtual void LogicUpdate() { }

    public virtual void PhysicalUpdate() { }

    public abstract void Exit();
    public virtual void HandleDamage(float Damage) { }
    public virtual bool CanEnter() => true;

    public virtual void EnableChange() {
        canChanged = true;
    }

}
