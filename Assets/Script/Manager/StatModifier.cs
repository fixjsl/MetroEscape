using UnityEngine;

public enum StatType
{
    Hungry, Thirst, Stamina, Speed
}
public class StatModifier
{
    public StatType Stat;
    public float multiplier;
    public object source;
    public StatModifier(StatType state, float multipier, object source)
    {
        Stat = state; this.multiplier = multipier; this.source = source;
    }
}
