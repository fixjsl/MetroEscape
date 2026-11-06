using System;
using System.Collections.Generic;
using UnityEngine;

public class ContainerObject : ItObjectBase
{
    [SerializeField] private LootTable lootTable;
    private bool open = false;
    private bool hasBeenOpened = false;
    private List<(ItemBase item, int count)> contents;

    public IReadOnlyList<(ItemBase item, int count)> Contents => contents;
    [SerializeField] private Transform RotatePivot;
    [SerializeField] private string openMessage = "열기";
    [SerializeField] private string closeMessage = "닫기";
    [SerializeField] private bool disappearAfterOpen = false;
    [SerializeField] private bool excludeFromCycle = false;

    [Header("Sound")]
    [SerializeField] private AudioClip firstOpenSound;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    public event Action ContentsChanged;

    public override string InteractMessage => Prompt(open ? closeMessage : openMessage);

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {

        if (open)
        {
            open = false;
            ContainerUI.Instance.Close();
            if (disappearAfterOpen && hasBeenOpened)
            {
                gameObject.SetActive(false);
                return;
            }
            RefreshPrompt();
            PlaySound(closeSound);
            return;
        }

        if (contents == null)
            contents = lootTable != null ? lootTable.Roll() : new List<(ItemBase, int)>();

        open = true;
        hasBeenOpened = true;
        ContainerUI.Instance.Open(this);
        RefreshPrompt();
        PlayInteractSound(firstOpenSound, openSound);
    }

    protected override void OnPlayerExited(PlayerStateMachine player)
    {
        open = false;
        ContainerUI.Instance.CloseKeepingInventory();

        if (disappearAfterOpen && hasBeenOpened)
            gameObject.SetActive(false);
    }

    public void CycleReset()
    {
        if (excludeFromCycle) return;
        if (disappearAfterOpen && !gameObject.activeSelf)
            gameObject.SetActive(true);
        open = false;
        hasBeenOpened = false;
        contents = null;
    }
    public bool UseAt(int index, PlayerStateMachine p)
    {
        if (contents == null || (uint)index >= (uint)contents.Count)
        {
            Debug.LogWarning($"[Container] 잘못된 칸 {index}", this);
            return false;
        }
        var (item, count) = contents[index];

        if (!item.OnUse(p)) return false;
        if (count <= 1) contents.RemoveAt(index);
        else contents[index] = (item, count - 1);
        ContentsChanged?.Invoke();
        return true;
    }

    public int TakeAt(int index, Inventory inv)
    {
        if (contents == null || (uint)index >= (uint)contents.Count)
        {
            Debug.LogWarning($"[Container] 잘못된 칸 {index}", this);
            return 0;
        }
        var (item, count) = contents[index];
        int added = inv.AddUpTo(item, count);
        if (added <= 0) return 0;

        if (added >= count) contents.RemoveAt(index);
        else contents[index] = (item, count - added);

        ContentsChanged?.Invoke();
        return added;
    }
}
