using UnityEngine;

public enum PerkAxis
{
    Combat, Gather, Explore
}

[CreateAssetMenu(fileName = "New Perk", menuName = "Perk")]
public class PerkBase : ScriptableObject
{
    public int PerkId;
    public string PerkName;
    public Sprite icon;
    [TextArea(2, 4)] public string description;

    [Header("해금 조건")]
    public PerkAxis axis;
    public int threshold;

    [Tooltip("수집 축에서만 사용. 지정하면 '이 아이템 N개', 비워두면 '서로 다른 N종'")]
    public ItemBase targetItem;

    [Header("효과")]
    public int maxHpBonus;
    public int maxStaminaBonus;
    public float carryWeightBonus;
    public ItemBase startingItem;



    public string GetConditionText()
    {
        switch (axis)
        {
            case PerkAxis.Combat:
                return $"몬스터 {threshold}마리 처치";
            case PerkAxis.Gather:
                return targetItem != null
                    ? $"{targetItem.name} {threshold}개 획득"
                    : $"서로 다른 아이템 {threshold}종 획득";
            case PerkAxis.Explore:
                return $"비상구로 {threshold}회 귀환";
            default:
                return "";
        }
    }

    public void Apply(PlayerStatus status, Inventory inventory)
    {
        status.Maxhp        += maxHpBonus;
        status.MaxStamina   += maxStaminaBonus;
        status.maxCarryWeight += carryWeightBonus;

        status.Water  = status.Water;
        status.Hungry = status.Hungry;
        if (startingItem != null) inventory.AddItem(startingItem, 1, false);
    }
}