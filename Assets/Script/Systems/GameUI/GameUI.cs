using UnityEngine;
public static class GameUI
{
    public static ICampfireWindow Campfire;
    public static IContainerWindow Container;
    public static UIPanel Crafting;            // 제작대는 열기/닫기만 해서 UI 틀(UIPanel) 그대로
    public static IConfirmDialog Confirm;
    public static IZoneSelectDialog ZoneSelect;
    public static IBossHud BossHud;

    // 도메인 리로드 꺼져 있어서 플레이 시작마다 비움
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Campfire = null;
        Container = null;
        Crafting = null;
        Confirm = null;
        ZoneSelect = null;
        BossHud = null;
    }
}
