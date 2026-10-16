using UnityEngine;

public class TimeManager
{
    private float curTime = 0;
    private readonly bool unscaled;

    public TimeManager(bool unscaled = false) => this.unscaled = unscaled;
    public void Reset() => curTime = 0;
    public bool Timer(float second)
    {
        curTime += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
        if (curTime >= second) {
            curTime = 0;
            return true;
        }
        return false;
    }
}
