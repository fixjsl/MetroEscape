using UnityEngine;

// 비상구 — 열쇠 소비 후 기지로 귀환 + 사이클 리셋 (1회성)
public class BaseReturnExit : ItObjectBase
{
    [SerializeField] private ItemBase keyItem;
    [SerializeField] private string hasKeyMessage  = "기지로 귀환";
    [SerializeField] private string noKeyMessage   = "비상구 열쇠가 필요";

    private bool used = false;

    public override bool isStuck => false;

    public override string InteractMessage
    {
        get
        {
            if (used) return "";
            bool hasKey = keyItem != null
                       && PlayerStateMachine.Instance != null
                       && PlayerStateMachine.Instance.inventory.slots.ContainsKey(keyItem.itemcode);
            string msg = hasKey ? hasKeyMessage : noKeyMessage;
            return $"{msg} [{InputBindings.Interact}]";
        }
    }

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        isInteracting = false;
        if (used) return;

        if (keyItem != null && !PlayerStateMachine.Instance.inventory.RemoveItem(keyItem, 1))
        {
            RefreshPrompt();
            return;
        }

        used = true;
        MetaProgressManager.Instance?.AddRun(); 
        CycleManager.Instance.ExecuteCycleReset();
    }

    // CycleManager가 새 위치로 이동시킨 뒤 호출
    public void ResetForCycle()
    {
        used = false;
        RefreshPrompt();
    }
}
