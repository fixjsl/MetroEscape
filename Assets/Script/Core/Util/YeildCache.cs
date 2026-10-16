using System.Collections.Generic;
using UnityEngine;

public static class YeildCache
{
    public static readonly Dictionary<float, WaitForSeconds> _intervals = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds GetIntervals(float interval)
    {
        if (!_intervals.ContainsKey(interval)){
            _intervals[interval] = new WaitForSeconds(interval);
        }
        return _intervals[interval];
    }


}
