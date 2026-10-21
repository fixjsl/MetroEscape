using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemBase> items = new();
    [SerializeField] private List<RecipeSO> recipes = new();
    private Dictionary<int, ItemBase> byCode;

    public IReadOnlyList<ItemBase> Items => items;
    public IReadOnlyList<RecipeSO> Recipes => recipes;
    public bool TryGet(int code, out ItemBase item) => byCode.TryGetValue(code, out item);
    public void Build()
    {
        foreach (var e in Validate()) Debug.LogError($"[ItemDatabase] {e}", this);
        byCode = new Dictionary<int, ItemBase>();
        foreach(ItemBase item in items)
        {
            if (item == null || item.itemcode == 0) continue; 
            byCode.TryAdd(item.itemcode, item);
        }
    }
    public List<string> Validate() 
    {
        var errors = new List<string>();
        var seen = new Dictionary<int, ItemBase>();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item == null) { errors.Add($"{i}번 칸이 비어 있음"); continue; }
            if (item.itemcode == 0) { errors.Add($"{item.name}: 코드 0은 '빈 칸'용으로 예약됨"); continue; }
            if (seen.TryGetValue(item.itemcode, out var other))
            {
                if (other == item) errors.Add($"{item.name}: 목록에 두 번 들어 있음");
                else errors.Add($"코드 {item.itemcode} 중복: {other.name}, {item.name}");
                continue;
            }
            seen[item.itemcode] = item;
        }
        foreach (var r in recipes)
        {
            if (r == null) { errors.Add("레시피 목록에 빈 칸"); continue; }
            if (r.result == null) errors.Add($"{r.name}: 결과 아이템 없음");
            if (r.resultcount <= 0) errors.Add($"{r.name}: 결과 개수 0 이하");
            foreach (var ing in r.ingredients)
                if (ing.item == null || ing.amount <= 0) errors.Add($"{r.name}: 재료 칸 비었거나 개수 0 이하");
        }
        return errors;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (var e in Validate()) Debug.LogError($"[ItemDatabase] {e}", this);
    }
#endif
}
