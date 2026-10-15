using UnityEngine;

public class StatMultiplierEffect : TimedEffect
{
    public StatType stat;
    public float multiplier;

    public override void OnStart(PlayerStateMachine player, ActiveEffect active)
    {
        var modifier = new StatModifier(stat, multiplier, active);
        player.status.AddModifier(modifier);
        active.state = modifier;
    }
    public override string Describe()
    {
        return $"Multiplies {stat} by {multiplier:F2} for {duration} seconds.";
    }

}
