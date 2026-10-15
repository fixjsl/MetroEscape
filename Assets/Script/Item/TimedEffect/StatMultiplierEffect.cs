using UnityEngine;

public class StatMultiplierEffect : TimedEffect
{
    public StatType stat;
    public float multiplier = 1f;

    public override void OnStart(IEffectTarget target, ActiveEffect active)
    {
        var modifier = new StatModifier(stat, multiplier, active);
        target.Modifiers.Add(modifier);
        active.state = modifier;
    }

    public override void OnEnd(IEffectTarget target, ActiveEffect active)
    {
        if (active.state is StatModifier modifier)
        {
            target.Modifiers.Remove(modifier);
        }
    }

    public override string Describe()
    {
        return $"{stat.ToKorean()} : x{multiplier:F2} 배 {duration}초 (중첩 불가)";
    }

}
