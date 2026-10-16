using UnityEngine;

public class StairTrigger : ItObjectBase
{
    [SerializeField] private StairObject stair;

    public override bool isStuck => false;
    public override string InteractMessage =>
        stair.IsOnStair
            ? $"하차 [{InputBindings.Interact}]"
            : $"계단 탑승 [{InputBindings.Interact}]";

    protected override void OnPlayerEntered(PlayerStateMachine player)
    {
        stair.OnPlayerEnterTrigger(this, player);
    }

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        isInteracting = false;
        stair.TryInteract(this, PlayerStateMachine.Instance);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }
}
