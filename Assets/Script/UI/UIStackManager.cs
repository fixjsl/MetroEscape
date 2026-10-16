using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class UIStackManager
{
    private readonly static List<UIPanel> panels = new();
    public static void Push(UIPanel p) { 
        Refresh();
        panels.Add(p);
    }
    public static void Pop(UIPanel p) { 
        Refresh();
        panels.Remove(p);
    }
    public static UIPanel Top { get; }  
    public static bool HandleCancel() {
        if (Top.CloseOnCancel)
        {
            return true;
        }
        else return false;
    }
    public static void Clear() {
        panels.Clear();
    }

    static void Refresh()
    {
        // 입력: BlocksCombatInput인 창이 하나라도 있으면 Attack/Dodge/Guard 끔 (기존 SetCombatInput 본문)
        // 시간: PausesTime인 창이 있으면 timeScale 0, 없으면 1
        // 커서: ShowsCursor인 창이 있으면 표시·해제, 없으면 게임 기본값으로 되돌림
    }
}
