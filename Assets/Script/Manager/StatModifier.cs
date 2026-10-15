using UnityEngine;

public enum StatType
{
    Hungry, Thirst, Stamina, Speed
}
public class StatModifier
{
    public readonly StatType Stat;
    public readonly float multiplier;
    public readonly object source;
    public StatModifier(StatType stat, float multiplier, object source)
    {
        Stat = stat; this.multiplier = multiplier; this.source = source;
    }
}
