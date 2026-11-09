// 캠프파이어 창 (CampfireUI가 구현)
public interface ICampfireWindow
{
    bool IsOpen { get; }
    void Open(Campfire campfire);
    void Close();
}
