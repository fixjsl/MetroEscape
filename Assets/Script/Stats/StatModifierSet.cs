using System.Collections.Generic;
using UnityEngine;

public class StatModifierSet
{
    private readonly List<StatModifier> modifiers = new List<StatModifier>();

    public void Add(StatModifier modifier)
    {
        modifiers.Add(modifier);
    }
    public void Remove(StatModifier modifier)
    {
        modifiers.Remove(modifier);
    }
    public float GetMultiplier(StatType type)
    {
        float up = 1f, down = 1f;
        foreach (var m in modifiers)
        {
            if (m.stat != type) continue;
            if (m.multiplier > 1f) up   = Mathf.Max(up,   m.multiplier);
            else                   down = Mathf.Min(down, m.multiplier);
        }
        return up * down;
    }
}


