using UnityEngine;

public interface IState
{
    public void Enter();

    public void LogicUpdate();
    public void PhysicalUpdate();

    public void Exit();
}
