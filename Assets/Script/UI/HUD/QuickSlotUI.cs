using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuickSlotUI : MonoBehaviour
{
    [SerializeField] private Image[] slotIcons = new Image[2];

    private Inventory inventory;

    void Start()
    {
        inventory = PlayerStateMachine.Instance.inventory;
        inventory.OnQuickSlotsChanged += Refresh;
        inventory.OnInventoryChanged += Refresh;
        Refresh();
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            inventory.UseQuickSlot(0, PlayerStateMachine.Instance);
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            inventory.UseQuickSlot(1, PlayerStateMachine.Instance);
    }

    void Refresh()
    {
        for (int i = 0; i < 2; i++)
        {
            int code = inventory.QuickSlots[i];
            if (code != 0 && ItemManager.itemDB.TryGetValue(code, out var item))
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
        if (inventory == null) return;
        inventory.OnQuickSlotsChanged -= Refresh;
        inventory.OnInventoryChanged -= Refresh;
    }
}
