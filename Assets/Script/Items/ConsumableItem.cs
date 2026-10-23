using System.Collections.Generic;
using UnityEngine;

public enum VitalStat {Hp,Hungry, Water, Stamina};
[System.Serializable]
public struct StatDalta
{
    public VitalStat stat;
    public float amount;
    public void ApplyTo(PlayerStatus status)
    {
        switch (stat)
        {
            case VitalStat.Hp: status.Hp += amount; break;
            case VitalStat.Stamina: status.Stamina += amount; break;
            case VitalStat.Hungry: status.Hungry += Mathf.RoundToInt(amount); break;
            case VitalStat.Water: status.Water += Mathf.RoundToInt(amount); break;
        }
    }
    public string Describe()
    {
        string label = stat switch
        {
            VitalStat.Hp => "HP",
            VitalStat.Hungry => "배고픔",
            VitalStat.Water => "수분",
            VitalStat.Stamina => "스태미나",
            _ => stat.ToString(),
        };
        return $"{label} {(amount > 0 ? "+" : "")}{amount}";   // 음수는 숫자에 이미 '-'가 붙어 있음
    }

}

[CreateAssetMenu(fileName = "ConsumableItem", menuName = "Scriptable Objects/Item/Consumable")]
public class ConsumableItem : ItemBase
{
    public List<StatDalta> instant = new();
    [SerializeReference] public List<TimedEffect> effects = new();

    public override bool OnUse(PlayerStateMachine player)
    {
        foreach( var i in instant)
        {
            i.ApplyTo(player.status);
        }
        foreach(var i in effects)
        {
            if (i == null) continue;
            player.effects.Apply(i, this);
        }
        return true;
    }
    public override string GetEffectDescription()
    {
        var lines = new List<string>();

        // 즉시 효과: 한 줄로
        var parts = new List<string>();
        foreach (var d in instant)
            if (d.amount != 0) parts.Add(d.Describe());   // 0은 "효과 없음"이라 생략
        if (parts.Count > 0) lines.Add(string.Join("  ", parts));

        // 시간제 효과: 효과마다 한 줄
        foreach (var e in effects)
            if (e != null) lines.Add(e.Describe());

        return string.Join("\n", lines);
    }
}
