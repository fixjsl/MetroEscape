using System.Collections.Generic;
using UnityEngine;

public enum ItemType { Weapon, Consumable, Material, Equipment, InteractObject, KeyItem }

[CreateAssetMenu(fileName = "ItemBase", menuName = "Scriptable Objects/ItemBase")]
public class ItemBase : ScriptableObject
{
    public Sprite icon;
    public Sprite firstAcquireImage;
    public int itemcode;
    public ItemType itemType;
    [TextArea(2, 4)] public string description;
    public int maxamount;
    public float weight;
    public bool canBurnAsFuel;

    public virtual bool OnUse(PlayerStateMachine player) { return true; }
    public virtual string GetEffectDescription() { return ""; }
    [System.Serializable]
    public struct CraftMaterial
    {
        public ItemBase item;
        public int amount;
    }
    public List<CraftMaterial> materials;

    public bool CanCraft(Inventory inventory)
    {
        foreach (var material in materials)
        {
            if (material.item == null) return false;
            if (!inventory.slots.TryGetValue(material.item.itemcode, out int count)) return false;
            if (count < material.amount) return false;
        }
        return true;
    }

    // 재료 차감과 산출물 지급을 하나의 단위로 처리한다.
    // 산출물을 인벤토리에 넣지 못하면(무게 초과 또는 슬롯 부족) 차감한 재료를 되돌리고 false를 반환한다.
    // 실패 사유의 안내는 Inventory가 발생시키는 OnInventoryFull / OnSlotsFull 이벤트가 담당한다.
    public bool Craft(Inventory inventory)
    {
        if (!CanCraft(inventory)) return false;

        foreach (var material in materials)
            inventory.RemoveItem(material.item, material.amount);

        if (inventory.AddItem(this)) return true;

        foreach (var material in materials)
            inventory.RestoreItem(material.item, material.amount);
        return false;
    }
}
