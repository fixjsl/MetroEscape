using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ContainerUI : UIPanel<ContainerUI>, IContainerWindow
{    
    [SerializeField] private Transform slotParent;
    [SerializeField] private ContainerSlotUI slotPrefab;

    private ContainerObject currentContents;
    
    private readonly List<ContainerSlotUI> activeSlots = new();
    bool keepInventoryOnClose;
    protected override void Awake()
    {
        base.Awake();
        GameUI.Container = this;
        root.SetActive(false);
    }

    public void Open(ContainerObject contents)
    {
        // 다른 상자 창이 열려 있으면 먼저 정리 (구독 해제 + 그 상자에 닫힘 알림)
        if (IsOpen && currentContents != contents) CloseKeepingInventory();
        currentContents = contents;
        currentContents.ContentsChanged += Refresh;
        if (!InventoryUI.Instance.IsOpen)
            InventoryUI.Instance.Open();
        Open();
    }
    protected override void OnOpened() => Refresh();

    protected override void OnClosed()
    {
        if (currentContents != null) currentContents.ContentsChanged -= Refresh;
        currentContents?.WindowClosed();
        currentContents = null;
        if(!keepInventoryOnClose)InventoryUI.Instance.Close();
        keepInventoryOnClose = false;
    }



    private void UseFromContainer(int index)
    {
        if (currentContents == null) return;
        currentContents.UseAt(index, PlayerStateMachine.Instance);

    }

    public void TakeItem(int index)
    {
        if (currentContents == null) return;
        if (currentContents.TakeAt(index, PlayerStateMachine.Instance.inventory)>0)

            InventoryUI.Instance.HideContextMenu();
    }

    private void Refresh()
    {
        foreach (var slot in activeSlots)
            Destroy(slot.gameObject);
        activeSlots.Clear();
        if (currentContents == null) return;

        var contents = currentContents.Contents;
        for (int i = 0; i < contents.Count; i++)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            int idx = i;
            var capturedItem = contents[i].item;
            slot.Setup(
                contents[i].item,
                contents[i].count,
                () => TakeItem(idx),
                pos => InventoryUI.Instance.ShowContextMenu(capturedItem, pos, () => UseFromContainer(idx), showDrop: false)
            );
            activeSlots.Add(slot);
        }
    }
    public void CloseKeepingInventory() { keepInventoryOnClose = true; Close(); }
}
