using System;
using System.Collections.Generic;

// 출발할 구역 고르는 창 (ZoneSelectUI가 구현)
public interface IZoneSelectDialog
{
    void Show(IReadOnlyList<ZoneEntry> zones, Action<ZoneEntry> callback);
}
