using UnityEngine;

public class IntervalTimer
{
    private float curTime = 0;
    private readonly bool unscaled;

    public IntervalTimer(bool unscaled = false) => this.unscaled = unscaled;
    public void Reset() => curTime = 0;
    public bool Timer(float second)
    {
        curTime += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
        if (curTime >= second) {
            curTime -= second;
            return true;
        }
        return false;
    }
}
