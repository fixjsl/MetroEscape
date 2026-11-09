using UnityEngine;

[System.Serializable]
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Assembly-CSharp")]   // 에셋에 저장된 옛 어셈블리 이름으로도 찾게 (asmdef 분리 때문)
public class HealOverTimeEffect : TimedEffect
{
    public float healPerSecond = 1f;

    public override void OnTick(IEffectTarget target, ActiveEffect active, float dt)
    {
        if (target is IDamageable d)
        {
            d.Heal(healPerSecond * dt);
        }
    }
    public override void OnReapply(ActiveEffect active)
    {
        active.remaining += duration;
    }

    public override string Describe()
    {
        return $"{duration}초 동안 매 초 {healPerSecond}만큼 회복 (재사용 시 시간 연장)";
    }
}
