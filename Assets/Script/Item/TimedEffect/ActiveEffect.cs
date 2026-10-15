using UnityEngine;

public class ActiveEffect
{
    public TimedEffect def;     // 어떤 효과인지 (에셋 안의 정의, 공유됨)
    public float remaining;     // 남은 시간
    public object state;        // 효과마다 필요한 실행 상태 (StatMultiplierEffect는 자기 StatModifier)
}
