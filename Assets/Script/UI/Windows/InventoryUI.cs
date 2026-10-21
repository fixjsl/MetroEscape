using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryUI : UIPanel<InventoryUI>
{
   
    [SerializeField] private Transform slotParent;
    [SerializeField] private ContainerSlotUI slotPrefab;
    [SerializeField] private Image weightFill;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private ContextMenuUI contextMenu;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private Inventory inventory;
    private readonly List<ContainerSlotUI> activeSlots = new();
    private InputAction _inventoryAction;

    void Start()
    {
        inventory = PlayerStateMachine.Instance.inventory;
        inventory.OnInventoryChanged += Refresh;
        root.SetActive(false);

        _inventoryAction = InputBindings.GetAction("Inventory");
        Debug.Log($"인벤토리 액션: {(_inventoryAction == null ? "null" : "정상")}");
        if (_inventoryAction != null)
            _inventoryAction.performed += OnInventoryPerformed;
    }

    private void OnInventoryPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("인벤토리 입력 들어옴");
        Toggle();
    }

    void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    
    protected override void OnOpened()
    {
        GameManager.Instance.PlaySFX(openSound);
        PositionByPlayerSide();
        Refresh();
        contextMenu.Hide();
    }

    [SerializeField] private float inventorySideOffset = 635f;

    private void PositionByPlayerSide()
    {
        float viewportX = Camera.main.WorldToViewportPoint(PlayerStateMachine.Instance.transform.position).x;
        var rt = (RectTransform)root.transform;

        // anchor/pivot은 인스펙터에서 center(0.5, 0.5)로 고정, 위치만 변경
        if (viewportX < 0.5f)
            rt.anchoredPosition = new Vector2(inventorySideOffset, 0f);   // 플레이어 왼쪽 → 인벤 오른쪽
        else
            rt.anchoredPosition = new Vector2(-inventorySideOffset, 0f);  // 플레이어 오른쪽 → 인벤 왼쪽
    }

    protected override void OnClosed()
    {
        GameManager.Instance.PlaySFX(closeSound);
        contextMenu.Hide();
        ItemInfoUI.Instance.Hide();
    }

    void Refresh()
    {
        foreach (var slot in activeSlots)
            Destroy(slot.gameObject);
        activeSlots.Clear();

        foreach (var (code, total) in inventory.slots)
        {
            if (!ItemManager.TryGet(code, out var item)) continue;
            var capturedItem = item;
            int stackmax = item.maxamount > 0 ? item.maxamount : total;     // 0 = 제한 없음 → 한 칸에 전부

            for (int left = total; left > 0; left -= stackmax)               // 12개, 최대 5 → 5, 5, 2
            {
                int n = Mathf.Min(left, stackmax);
                var slot = Instantiate(slotPrefab, slotParent);
                slot.Setup(item, n, null, pos => ShowContextMenu(capturedItem, pos));
                activeSlots.Add(slot);
            }
        }

        var status = PlayerStateMachine.Instance.status;
        float max = status.curMaxCarryWeight;
        float current = inventory.currentWeight;
        if (weightFill != null) weightFill.fillAmount = max > 0 ? current / max : 0f;
        if (weightText != null) weightText.text = $"{current:F1} / {max:F1}";
    }

    public void ShowContextMenu(ItemBase item, Vector2 screenPos, System.Action onUse = null, bool showDrop = true)
    {
        ItemInfoUI.Instance.Hide();
        contextMenu.Show(item, screenPos, inventory, onUse, showDrop);
    }

    public void HideContextMenu() => contextMenu.Hide();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;
        if (_inventoryAction != null)
            _inventoryAction.performed -= OnInventoryPerformed;
    }
}
