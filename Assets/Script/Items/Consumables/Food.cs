using UnityEngine;

[CreateAssetMenu(fileName = "HealItem", menuName = "Scriptable Objects/Item/Food")]
public class Food : ItemBase
{
    public int FillingAmount;
    public int HealthAmount;

    private void Reset() => itemType = ItemType.Consumable;

    public override bool OnUse(PlayerStateMachine player)
    {
        player.status.Hungry += FillingAmount;
        player.status.Hp += HealthAmount;
        return true;
    }

    public override string GetEffectDescription() => HealthAmount < 0 ? $"배고픔 +{FillingAmount}, 체력 {HealthAmount}" : $"배고픔 +{FillingAmount}";
}