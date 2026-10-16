using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ContainerUI : MonoBehaviour
{
    public static ContainerUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [SerializeField] private GameObject panel;
    [SerializeField] private Transform slotParent;
    [SerializeField] private ContainerSlotUI slotPrefab;

    private List<(ItemBase item, int count)> currentContents;
    
    private readonly List<ContainerSlotUI> activeSlots = new();

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public bool IsOpen => panel.activeSelf;

    public void Open(List<(ItemBase item, int count)> contents)
    {
        currentContents = contents;
        Refresh();
        panel.SetActive(true);
        InventoryUI.Instance.Open();
        SetCombatInput(false);
    }

    public void Close()
    {
        panel.SetActive(false);
        currentContents = null;
        InventoryUI.Instance.Close();
        SetCombatInput(true);
    }

    private void SetCombatInput(bool enabled)
    {
        var toggle = enabled
            ? (Action<InputAction>)(a => a.Enable())
            : (a => a.Disable());

        toggle(InputBindings.GetAction("Attack"));
        toggle(InputBindings.GetAction("Dodge"));
        toggle(InputBindings.GetAction("Guard"));
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

        if (!PlayerStateMachine.Instance.inventory.AddItem(item, count))
        {
            Debug.LogWarning($"[Container] AddItem 실패 — {item.name} x{count} | 현재무게: {PlayerStateMachine.Instance.inventory.currentWeight} | 슬롯수: {PlayerStateMachine.Instance.inventory.slots.Count}");
            return;
        }

        currentContents.RemoveAt(index);
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
}
