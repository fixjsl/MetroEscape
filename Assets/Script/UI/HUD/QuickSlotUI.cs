using UnityEngine;
using UnityEngine.UI;

public class QuickSlotUI : MonoBehaviour
{
    [SerializeField] private Image[] slotIcons = new Image[2];

    private Inventory inventory;
    private PlayerInput input;

    void Start()
    {
        inventory = PlayerStateMachine.Instance.inventory;
        inventory.OnQuickSlotsChanged += Refresh;
        inventory.OnInventoryChanged += Refresh;
        // 숫자키는 PlayerInput이 읽어서 알려줌 (입력을 한 곳에서만 읽기)
        input = PlayerStateMachine.Instance.input;
        input.QuickSlotPressed += UseSlot;
        Refresh();
    }

    void UseSlot(int index) => inventory.UseQuickSlot(index, PlayerStateMachine.Instance);

    void Refresh()
    {
        for (int i = 0; i < 2; i++)
        {
            int code = inventory.QuickSlots[i];
            if (code != 0 && ItemManager.TryGet(code, out var item))
            {
                slotIcons[i].sprite = item.icon;
                slotIcons[i].color = Color.white;
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }

    void OnDestroy()
    {
        if (input != null) input.QuickSlotPressed -= UseSlot;
        if (inventory == null) return;
        inventory.OnQuickSlotsChanged -= Refresh;
        inventory.OnInventoryChanged -= Refresh;
    }
}
