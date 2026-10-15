using UnityEngine;

public enum StatType
{
    Hungry, Thirst, Stamina, Speed
}
public class StatModifier
{
    public readonly StatType stat;
    public readonly float multiplier;
    public readonly object source;
    public StatModifier(StatType stat, float multiplier, object source)
    {
        this.stat = stat; this.multiplier = multiplier; this.source = source;
    }
}
public static class StatTypeExtensions
{
    public static string ToKorean(this StatType stat) => stat switch
    {
        StatType.Hungry => "허기 감소 속도",
        StatType.Thirst => "수분 감소 속도",
        StatType.Stamina => "스태미나 회복",
        StatType.Speed => "이동 속도",
        _ => stat.ToString(),   // 새 항목을 추가하고 깜빡했을 때 대비
    };
}