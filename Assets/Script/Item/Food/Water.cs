using UnityEngine;

[CreateAssetMenu(fileName = "HealItem", menuName = "Scriptable Objects/Item/Water")]
public class Water : ItemBase
{
    public int FillingAmount;

    private void Reset() => itemType = ItemType.Consumable;

    public override bool OnUse(PlayerStateMachine player)
    {
        player.status.Water += FillingAmount;
        return true;
    }

    public override string GetEffectDescription() => $"수분 +{FillingAmount}";
}