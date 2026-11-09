// 상자·시체 내용물 창 (ContainerUI가 구현)
public interface IContainerWindow
{
    void Open(ContainerObject contents);
    void Close();
    void CloseKeepingInventory();
}
