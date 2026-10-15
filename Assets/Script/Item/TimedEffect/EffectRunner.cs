using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EffectRunner
{
    private readonly IEffectTarget target;
    private readonly List<ActiveEffect> activeEffects = new List<ActiveEffect>();

    public EffectRunner(IEffectTarget target)
    {
        this.target = target;
    }
    public IReadOnlyList<ActiveEffect> ActiveEffects => activeEffects;

    public void Apply(TimedEffect def, object source)
    {
        foreach (var a in activeEffects)
        {
            if (a.def == def)
            {
                a.def.OnReapply(a);
                return;
            }
        }
        var newEffect = new ActiveEffect (def,source, def.duration );
        activeEffects.Add(newEffect);
        def.OnStart(target, newEffect);
    }
    public void Tick(float dt)
    {
        for(int i = activeEffects.Count - 1; i >= 0; i--)
        {
            
            var a = activeEffects[i];
            a.def.OnTick(target, a, dt);
            if (a.def.IsInfinite) continue;
            a.remaining -= dt;
            if (a.remaining <= 0f)
            {
                a.def.OnEnd(target, a);
                activeEffects.RemoveAt(i);
            }
        }
    }
    public void ClearOnDeath()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var a = activeEffects[i];
            if (a.def.removeOnDeath)
            {
                a.def.OnEnd(target, a);
                activeEffects.RemoveAt(i);
            }
        }
    }
    public void ClearAll()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var a = activeEffects[i];
            a.def.OnEnd(target, a);
            activeEffects.RemoveAt(i);
        }
    }
}
