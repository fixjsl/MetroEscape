using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ContainerUI : UIPanel<ContainerUI>
{    
    [SerializeField] private Transform slotParent;
    [SerializeField] private ContainerSlotUI slotPrefab;

    private List<(ItemBase item, int count)> currentContents;
    
    private readonly List<ContainerSlotUI> activeSlots = new();
    bool keepInventoryOnClose;
    protected override void Awake()
    {
        base.Awake();
        root.SetActive(false);
    }

    public void Open(List<(ItemBase item, int count)> contents)
    {
        currentContents = contents;
        if(!InventoryUI.Instance.IsOpen)
            InventoryUI.Instance.Open();
        Open();
    }
    protected override void OnOpened() => Refresh();

    protected override void OnClosed()
    {
        currentContents = null;
        if(!keepInventoryOnClose)InventoryUI.Instance.Close();
        keepInventoryOnClose = false;
    }



    private void UseFromContainer(int index)
    {
        if (currentContents == null || index >= currentContents.Count) return;
        var (item, count) = currentContents[index];
        item.OnUse(PlayerStateMachine.Instance);
        if (count <= 1)
            currentContents.RemoveAt(index);
        else
            currentContents[index] = (item, count - 1);
        Refresh();

    }

    public void TakeItem(int index)
    {
        if (currentContents == null || index >= currentContents.Count) return;
        var (item, count) = currentContents[index];

        int added = PlayerStateMachine.Instance.inventory.AddUpTo(item, count);

        if (added <= 0) return;                                   // ① 하나도 못 넣음 → 상자 그대로 (안내는 AddUpTo가 이미 띄움)

        if (added >= count)
            currentContents.RemoveAt(index);                      // ② 전부 넣음 → 상자에서 칸 삭제
        else
            currentContents[index] = (item, count - added);       // ③ 일부만 → 남은 개수로

        InventoryUI.Instance.HideContextMenu();
        Refresh();
    }

    private void Refresh()
    {
        foreach (var slot in activeSlots)
            Destroy(slot.gameObject);
        activeSlots.Clear();

        for (int i = 0; i < currentContents.Count; i++)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            int idx = i;
            var capturedItem = currentContents[i].item;
            slot.Setup(
                currentContents[i].item,
                currentContents[i].count,
                () => TakeItem(idx),
                pos => InventoryUI.Instance.ShowContextMenu(capturedItem, pos, () => UseFromContainer(idx), showDrop: false)
            );
            activeSlots.Add(slot);
        }
    }
    public void CloseKeepingInventory() { keepInventoryOnClose = true; Close(); }
}
