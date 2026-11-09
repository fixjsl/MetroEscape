using UnityEngine;
[System.Serializable]
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Assembly-CSharp")]   // 에셋에 저장된 옛 어셈블리 이름으로도 찾게 (asmdef 분리 때문)
public abstract class TimedEffect
{
    [Tooltip("0 이하 = 무한 지속")]
    public float duration = 60f;
    public bool removeOnDeath = true;

    public bool IsInfinite => duration <= 0f;
    public virtual void OnStart(IEffectTarget target, ActiveEffect active) { }
    public virtual void OnTick(IEffectTarget target, ActiveEffect active, float dt) { }
    public virtual void OnEnd(IEffectTarget target, ActiveEffect active) { }
    public virtual void OnReapply(ActiveEffect active) => active.remaining = duration; // 기본: 시간 초기화
    public abstract string Describe();
}
