using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TimedEffectItem", menuName = "Scriptable Objects/Item/TimedEffectItem")]
public class TimedEffectItem : ItemBase
{
    private void Reset() => itemType = ItemType.Consumable;

    [SerializeReference] public List<TimedEffect> effects = new List<TimedEffect>();

    public override string GetEffectDescription()
    {
        if (effects == null || effects.Count == 0) return "";

        var lines = new List<string>();
        foreach (var e in effects)
        {
            if (e == null) continue;          // 인스펙터에서 빈 칸(null)이 들어가 있을 때 대비
            lines.Add(e.Describe());
        }
        return string.Join("\n", lines);
    }

    public override bool OnUse(PlayerStateMachine player)
    {
        foreach (var effect in effects)
        {
            if(effect == null) continue;
            player.effects.Apply(effect, this);
        }
        return true;
    }

}
