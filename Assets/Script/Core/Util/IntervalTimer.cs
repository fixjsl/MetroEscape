using UnityEngine;

public class IntervalTimer
{
    private float curTime = 0;
    private readonly bool unscaled;

    public IntervalTimer(bool unscaled = false) => this.unscaled = unscaled;
    public void Reset() => curTime = 0;
    public bool Timer(float second) => Timer(second, unscaled ? Time.unscaledDeltaTime : Time.deltaTime);

    // 지나간 시간을 직접 넣는 버전 (테스트용, 게임에서는 위 버전을 씀)
    public bool Timer(float second, float dt)
    {
        curTime += dt;
        if (curTime >= second) {
            curTime -= second;
            return true;
        }
        return false;
    }
}
