using UnityEngine;

public class Interact : PlayerState
{

    private Object interactObject;
    public Interact(PlayerStateMachine stateMachine) : base(stateMachine) {
    }
    public override void Enter()
    {
        if (player.interact.Closest == null)
        {
            Debug.LogWarning("Interact: 상호작용 대상이 없습니다.");
            canChanged = true;
            return;
        }
        player.interact.Closest.OnInteract(player.transform.position);
    }

    public override void Exit()
    {
        canChanged = true;
    }

}
