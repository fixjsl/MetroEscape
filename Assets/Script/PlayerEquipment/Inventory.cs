using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private PlayerStatus status;
    public Dictionary<int, int> slots { get; private set; } = new Dictionary<int, int>();
    public float currentWeight { get; private set; }
    public int[] QuickSlots { get; private set; } = new int[2];

    public event Action OnInventoryChanged;
    public event Action OnQuickSlotsChanged;
    public event Action<ItemBase> OnFirstAcquire;
    public event Action OnInventoryFull;
    public event Action OnSlotsFull;
    private HashSet<int> acquiredCodes = new HashSet<int>();

    public Inventory(PlayerStatus status)
    {
        this.status = status;
    }

    public bool AddItem(ItemBase itemBase, int num = 1, bool suppressFirstAcquire = false, bool countAsGathered = true)
    {
        int key = itemBase.itemcode;
        float addWeight = itemBase.weight * num;
        if (currentWeight + addWeight > status.curMaxCarryWeight)
        {
            OnInventoryFull?.Invoke();
            return false;
        }

        if (slots.TryGetValue(key, out int count))
        {
            slots[key] = count + num;
            currentWeight += addWeight;
            if (countAsGathered)
                MetaProgressManager.Instance?.AddGather(itemBase, num);   // ← 추가
            OnInventoryChanged?.Invoke();
            return true;
        }
        if (slots.Count >= (int)status.maxSlots)
        {
            OnSlotsFull?.Invoke();
            return false;
        }
        slots[key] = num;
        currentWeight += addWeight;
        if (countAsGathered)
            MetaProgressManager.Instance?.AddGather(itemBase, num);       // ← 추가
        if (!suppressFirstAcquire)
        {
            bool isNew = acquiredCodes.Add(key);
            if (isNew && itemBase.firstAcquireImage != null)
                OnFirstAcquire?.Invoke(itemBase);
        }
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemBase itemBase, int num = 1)
    {
        int key = itemBase.itemcode;
        if (!slots.TryGetValue(key, out int count)) return false;
        if (count < num) return false;

        if (count == num)
        {
            slots.Remove(key);
            ClearQuickSlotIfEmpty(key);
        }
        else
            slots[key] = count - num;

        currentWeight -= itemBase.weight * num;
        OnInventoryChanged?.Invoke();
        return true;
    }

    // 취소된 조작을 되돌릴 때만 사용한다.
    // 직전에 이 인벤토리에서 차감한 수량을 그대로 복원하는 용도이므로 무게·슬롯 한도를 검사하지 않으며,
    // 최초 획득 연출과 수집 지표에도 반영하지 않는다. 일반적인 아이템 획득에는 AddItem을 사용한다.
    public void RestoreItem(ItemBase itemBase, int num = 1)
    {
        if (itemBase == null || num <= 0) return;

        int key = itemBase.itemcode;
        if (slots.TryGetValue(key, out int count)) slots[key] = count + num;
        else slots[key] = num;

        currentWeight += itemBase.weight * num;
        OnInventoryChanged?.Invoke();
    }

    public void UseItem(ItemBase itemBase, PlayerStateMachine player)
    {
        if (!slots.ContainsKey(itemBase.itemcode)) return;
        bool consume = itemBase.OnUse(player);
        if (consume) RemoveItem(itemBase, 1);
    }

    public void DropItem(ItemBase itemBase, int num = 1) => RemoveItem(itemBase, num);

    public void SetQuickSlot(int slotIndex, int itemCode)
    {
        if (slotIndex < 0 || slotIndex >= QuickSlots.Length) return;
        QuickSlots[slotIndex] = itemCode;

        OnQuickSlotsChanged?.Invoke();
    }

    public void UseQuickSlot(int slotIndex, PlayerStateMachine player)
    {
        if (slotIndex < 0 || slotIndex >= QuickSlots.Length) return;
        int code = QuickSlots[slotIndex];
        if (code == 0 || !slots.ContainsKey(code)) { QuickSlots[slotIndex] = 0; return; }
        if (!ItemManager.itemDB.TryGetValue(code, out var item))
        {
            Debug.LogWarning($"[Inventory] itemcode {code} not found in itemDB");
            QuickSlots[slotIndex] = 0;
            OnQuickSlotsChanged?.Invoke();
            return;
        }
        UseItem(item, player);
        OnQuickSlotsChanged?.Invoke();
    }

    private void ClearQuickSlotIfEmpty(int itemCode)
    {
        for (int i = 0; i < QuickSlots.Length; i++)
        {
            if (QuickSlots[i] == itemCode)
            {
                QuickSlots[i] = 0;
                OnQuickSlotsChanged?.Invoke();
            }
        }
    }

    public void LoseOnDeath(float rate)
{
    
    if (rate <= 0f || slots.Count == 0) return;

    var kinds  = new List<ItemBase>();   // 비유니크 종류
    var counts = new List<int>();
    var uniques = new List<(ItemBase item, int count)>();
    int total = 0, uniqueTotal = 0;

    foreach (var pair in slots)
    {
        
        if (!ItemManager.itemDB.TryGetValue(pair.Key, out var item)) continue;
        if(item.itemType == ItemType.KeyItem) continue; // 키 아이템은 소실되지 않음
        total += pair.Value;
        if (LootTable.IsUniqueItem(pair.Key)) { uniques.Add((item, pair.Value)); uniqueTotal += pair.Value; }
        else { kinds.Add(item); counts.Add(pair.Value); }
    }
    if (total == 0) return;

    int target = Mathf.FloorToInt(total * rate);

    // 유니크는 전량 소실
    foreach (var (item, count) in uniques) RemoveItem(item, count);

    // 남은 몫을 수량 가중으로 뽑되, 결과만 누적한다
    int pool = total - uniqueTotal;
    int remain = Mathf.Clamp(target - uniqueTotal, 0, pool);
    var loss = new int[kinds.Count];

    for (int n = 0; n < remain; n++)
    {
        int pick = UnityEngine.Random.Range(0, pool);
        for (int i = 0; i < kinds.Count; i++)
        {
            int avail = counts[i] - loss[i];
            if (pick < avail) { loss[i]++; pool--; break; }
            pick -= avail;
        }
    }

    // 실제 제거는 종류마다 한 번
    for (int i = 0; i < kinds.Count; i++)
        if (loss[i] > 0) RemoveItem(kinds[i], loss[i]);
}
}
