using UnityEngine;

public class ItemPickup : ItObjectBase
{
    [SerializeField] private ItemBase item;
    [SerializeField] private int count = 1;

    public override string InteractMessage => Prompt(item != null ? $"{item.name} 줍기" : "줍기");

    public void Setup(ItemBase item, int count)
    {
        this.item = item;
        this.count = count;
    }

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        if (item == null) return;
        int added = PlayerStateMachine.Instance.inventory.AddUpTo(item, count);
        if (added >= count) { gameObject.SetActive(false); return; }
        count -= added;            // 일부만 들어갔으면 남은 개수로 바닥에 남김
        RefreshPrompt();
    }
}
