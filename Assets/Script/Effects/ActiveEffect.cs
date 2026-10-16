using UnityEngine;
using UnityEngine.InputSystem.HID;

public class ActiveEffect
{
    public TimedEffect def;     // 어떤 효과인지 (에셋 안의 정의, 공유됨)
    public object source;       //이 효과를 건 것(TimedEffectItem, 나중에 PerkBase 등). HUD 묶음·아이콘 기준
    public float remaining;     // 남은 시간
    public object state;        // 효과마다 필요한 실행 상태 (StatMultiplierEffect는 자기 StatModifier)

    public ActiveEffect(TimedEffect def, object source, float remaining)
    {
        this.def = def;
        this.source = source;
        this.remaining = remaining;
    }
}
