using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class UIStackManager
{
    private readonly static List<UIPanel> panels = new();
    static bool pausedByUI;
    public static void Push(UIPanel p) { 
        panels.Add(p);
        Refresh();
    }
    public static void Pop(UIPanel p) { 
        panels.Remove(p);
        Refresh();
    }
    public static UIPanel Top => panels.Count > 0 ? panels[panels.Count - 1] : null;
    public static bool HandleCancel() {
        var top = Top;
        if (top == null) return false;
        if (top.CloseOnCancel) top.Close();
        return true;
    }
    static void SetCombatInput(bool enabled)
    {
        var toggle = enabled
            ? (Action<InputAction>)(a => a.Enable())
            : (a => a.Disable());

        toggle(InputBindings.GetAction("Attack"));
        toggle(InputBindings.GetAction("Dodge"));
        toggle(InputBindings.GetAction("Guard"));
    }
    public static void Clear() {
        if (pausedByUI) Time.timeScale = 1f;
        panels.Clear();
        pausedByUI = false;
        SetCombatInput(true);
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {

        panels.Clear();
        pausedByUI = false;
    }
    static void Refresh()
    {
        bool block = false, pause = false;
        // 입력: BlocksCombatInput인 창이 하나라도 있으면 Attack/Dodge/Guard 끔 (기존 SetCombatInput 본문)
        foreach (var i in panels) { if (i.BlocksCombatInput) block = true; if (i.PausesTime) pause = true; }

        SetCombatInput(!block);
        // 시간: PausesTime인 창이 있으면 timeScale 0, 없으면 1
        if (pause && !pausedByUI) { Time.timeScale = 0f; pausedByUI = true; }
        else if (!pause && pausedByUI) { Time.timeScale = 1f; pausedByUI = false; }
        
    }
}
