using UnityEngine;
//상호작용가능 장애물 코드
public class Blockade : ItObjectBase
{
    [SerializeField] private ItemBase keyItem;
    [SerializeField] private string noKeyMessage = "열쇠 필요";
    [SerializeField] private string hasKeyMessage = "다음 구역 해방";

    public override bool isStuck => false;
     protected bool HasKey =>
        keyItem != null &&
        PlayerStateMachine.Instance != null &&
        PlayerStateMachine.Instance.inventory.slots.ContainsKey(keyItem.itemcode);
    public override string InteractMessage
        => $"{(HasKey ? hasKeyMessage : noKeyMessage)} [{InputBindings.Interact}]";

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        isInteracting = false;

        if (!HasKey)
        {
            
            RefreshPrompt();
            return;
        }

        BlockadeInteract();
    }

    protected virtual void BlockadeInteract()
    {
        PlayerStateMachine.Instance.inventory.RemoveItem(keyItem, 1);
        gameObject.SetActive(false);
    }
}
