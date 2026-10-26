using UnityEngine;

public class StairTrigger : ItObjectBase
{
    [SerializeField] private StairObject stair;

    public override string InteractMessage =>
        Prompt(stair.IsOnStair ? "하차" : "계단 탑승");

    protected override void OnPlayerEntered(PlayerStateMachine player)
    {
        stair.OnPlayerEnterTrigger(this, player);
    }

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        stair.TryInteract(this, PlayerStateMachine.Instance);
    }
}
