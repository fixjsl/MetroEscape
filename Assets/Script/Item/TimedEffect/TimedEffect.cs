using UnityEngine;
[System.Serializable]
public abstract class TimedEffect
{
    public float duration = 60f; // 지속 시간
    public virtual void OnStart(IEffectTarget target, ActiveEffect active) { }
    public virtual void OnTick(IEffectTarget target, ActiveEffect active, float dt) { }
    public virtual void OnEnd(IEffectTarget target, ActiveEffect active) { }
    public virtual void OnReapply(ActiveEffect active) => active.remaining = duration; // 기본: 시간 초기화
    public abstract string Describe();
}
