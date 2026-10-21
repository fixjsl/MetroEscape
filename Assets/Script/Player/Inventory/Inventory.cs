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
    public event Action OnInventoryFull;
    public event Action OnSlotsFull;

    public Inventory(PlayerStatus status)
    {
        this.status = status;
    }
    private static int SlotsFor(ItemBase item, int count)
    {
        if (count <= 0) return 0;
        int max = item.maxamount;
        if (max <= 0) return 1;                        // 0 = 제한 없음 → 1칸에 다 쌓임
        return (count + max - 1) / max;                // 올림 나눗셈: 12개, 5 → 3
    }
    public int UsedSlots
    {
        get
        {
            int used = 0;
            foreach (var (code, count) in slots)
                if (ItemManager.TryGet(code, out var item))
                    used += SlotsFor(item, count);
            return used;
        }
    }
    public int CanAdd(ItemBase item, int num)
    {
        int weightCanNum;
        float remain = status.curMaxCarryWeight - currentWeight;
        weightCanNum = Mathf.FloorToInt(remain / item.weight + 0.0001f);   // 오차 여유
        weightCanNum = Mathf.Max(0, weightCanNum);
        slots.TryGetValue(item.itemcode, out int cur);                     // 없으면 cur = 0
        int free = Mathf.Max(0, (int)status.maxSlots - UsedSlots);
        int max = item.maxamount;

        int slotCanNum;
        if (max <= 0)                                                      // 0 = 한 칸에 무제한
            slotCanNum = (cur > 0 || free > 0) ? num : 0;                  // 이미 있거나 빈 칸이 있으면 전부
        else
            slotCanNum = (SlotsFor(item, cur) + free) * max - cur;

        return Mathf.Min(num, weightCanNum, slotCanNum);
    }
    public int AddUpTo(ItemBase item, int num,bool countAsGathered = true)
    {
        int n = CanAdd(item, num);

        if (n < num)                                   // 다 못 넣음 → 왜 막혔는지 안내
        {
            NotifyFull(item, n);
        }
        if (n <= 0) return 0;

        slots.TryGetValue(item.itemcode, out int cur);
        slots[item.itemcode] = cur + n;
        currentWeight += item.weight * n;

        if (countAsGathered)
            MetaProgressManager.Instance?.AddGather(item, n);    // 넣은 만큼만 채집으로 셈
        OnInventoryChanged?.Invoke();
        return n;
    }
    private void NotifyFull(ItemBase item, int fitted)
    {
        bool weightFull = currentWeight + item.weight * (fitted + 1) > status.curMaxCarryWeight + 0.0001f;
        if (weightFull) OnInventoryFull?.Invoke();
        else OnSlotsFull?.Invoke();
    }

    public bool AddItem(ItemBase item, int num = 1, bool countAsGathered = true)
    {
        int n = CanAdd(item, num);
        if (n < num) { NotifyFull(item, n); return false; }     // 전부 아니면 아무것도 안 넣음
        AddUpTo(item, num, countAsGathered);
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
        if (!ItemManager.TryGet(code, out var item))
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
        
        if (!ItemManager.TryGet(pair.Key, out var item)) continue;
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
