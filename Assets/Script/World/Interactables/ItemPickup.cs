using UnityEngine;

public class ItemPickup : ItObjectBase
{
    [SerializeField] private ItemBase item;
    [SerializeField] private int count = 1;

    public override bool isStuck => false;
    public override string InteractMessage => item != null ? $"{item.name} 줍기  [F]" : "줍기  [F]";

    public void Setup(ItemBase item, int count)
    {
        this.item = item;
        this.count = count;
    }

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        if (item == null) return;
        if (PlayerStateMachine.Instance.inventory.AddItem(item, count))
            gameObject.SetActive(false);
        else
            isInteracting = false;
    }
}
