using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { Weapon, Consumable, Material, Equipment, InteractObject, KeyItem }

[CreateAssetMenu(fileName = "ItemBase", menuName = "Scriptable Objects/ItemBase")]
public class ItemBase : ScriptableObject
{
    public Sprite icon;
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


    // 재료 차감과 산출물 지급을 하나의 단위로 처리한다.
    // 산출물을 인벤토리에 넣지 못하면(무게 초과 또는 슬롯 부족) 차감한 재료를 되돌리고 false를 반환한다.
    // 실패 사유의 안내는 Inventory가 발생시키는 OnInventoryFull / OnSlotsFull 이벤트가 담당한다.



}
