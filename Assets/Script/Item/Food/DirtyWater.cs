using UnityEngine;

[CreateAssetMenu(fileName = "DirtyWater", menuName = "Scriptable Objects/Item/DirtyWater")]
public class DirtyWater : ItemBase
{
    public int waterAmount = 10;
    public float hpChange = -1f;
    public int hungryChange = -10;

    private void Reset() => itemType = ItemType.Consumable;

    public override bool OnUse(PlayerStateMachine player)
    {
        player.status.Water += waterAmount;
        player.status.Hp += hpChange;
        player.status.Hungry += hungryChange;
        return true;
    }

    public override string GetEffectDescription()
    {
        var parts = new System.Collections.Generic.List<string>();
        if (waterAmount != 0) parts.Add($"수분 {(waterAmount > 0 ? "+" : "")}{waterAmount}");
        if (hpChange != 0)    parts.Add($"HP {(hpChange > 0 ? "+" : "")}{hpChange}");
        if (hungryChange != 0) parts.Add($"배고픔 {(hungryChange > 0 ? "+" : "")}{hungryChange}");
        return string.Join("  ", parts);
    }
}
