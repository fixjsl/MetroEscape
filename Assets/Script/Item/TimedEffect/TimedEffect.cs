using UnityEngine;
[System.Serializable]
public abstract class TimedEffect
{
    public float duration = 60f; // 지속 시간
    public virtual void OnStart(PlayerStateMachine player, ActiveEffect active) { }
    public virtual void OnTick(PlayerStateMachine player, ActiveEffect active, float dt) { }
    public virtual void OnEnd(PlayerStateMachine player, ActiveEffect active) { }
    public virtual void OnReapply(ActiveEffect active) => active.remaining = duration; // 기본: 시간 초기화
    public abstract string Describe();
}
