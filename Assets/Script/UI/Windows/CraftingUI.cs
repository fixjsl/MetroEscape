using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CraftingUI : MonoBehaviour
{
    public static CraftingUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [SerializeField] private GameObject panel;

    [Header("탭 버튼 (0:무기 1:방어구 2:식료품 3:상위재료)")]
    [SerializeField] private Button[] tabButtons;

    [Header("레시피 목록")]
    [SerializeField] private Transform recipeListParent;
    [SerializeField] private CraftingSlotUI craftingSlotPrefab;

    [Header("상세 패널")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image selectedIcon;
    [SerializeField] private TMP_Text selectedNameText;
    [SerializeField] private TMP_Text selectedDescText;
    [SerializeField] private Transform materialListParent;
    [SerializeField] private CraftingMaterialUI materialPrefab;
    [SerializeField] private Button craftButton;

    private static readonly ItemType[] TabTypes =
        { ItemType.Weapon, ItemType.Equipment, ItemType.Consumable, ItemType.Material };

    private int currentTab;
    private ItemBase selectedItem;
    private readonly List<CraftingSlotUI> activeSlots = new();
    private readonly List<CraftingMaterialUI> activeMaterials = new();

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int idx = i;
            tabButtons[i].onClick.AddListener(() => SelectTab(idx));
        }
        craftButton.onClick.AddListener(OnCraftClicked);
    }

    public bool IsOpen => panel.activeSelf;

    public void Open()
    {
        panel.SetActive(true);
        SelectTab(0);
        SetCombatInput(false);
    }

    public void Close()
    {
        panel.SetActive(false);
        selectedItem = null;
        SetCombatInput(true);
    }

    private void SetCombatInput(bool enabled)
    {
        System.Action<InputAction> toggle = enabled ? a => a.Enable() : a => a.Disable();
        toggle(InputBindings.GetAction("Attack"));
        toggle(InputBindings.GetAction("Dodge"));
        toggle(InputBindings.GetAction("Guard"));
    }

    private void SelectTab(int index)
    {
        currentTab = index;
        selectedItem = null;
        detailPanel.SetActive(false);
        RefreshList();
    }

    private void RefreshList()
    {
        foreach (var slot in activeSlots) Destroy(slot.gameObject);
        activeSlots.Clear();

        var type = TabTypes[currentTab];

        foreach (var (_, item) in ItemManager.itemDB)
        {
            bool typeMatch = item.itemType == type ||
                              (type == ItemType.Consumable && item.itemType == ItemType.InteractObject);
            if (!typeMatch) continue;
            if (item.materials == null || item.materials.Count == 0) continue;

            var slot = Instantiate(craftingSlotPrefab, recipeListParent);
            var captured = item;
            slot.Setup(item, () => SelectItem(captured));
            activeSlots.Add(slot);
        }
    }

    private void SelectItem(ItemBase item)
    {
        selectedItem = item;
        detailPanel.SetActive(true);

        if (selectedIcon != null) selectedIcon.sprite = item.icon;
        if (selectedNameText != null) selectedNameText.text = item.name;
        if (selectedDescText != null) selectedDescText.text = item.description;

        foreach (var m in activeMaterials) Destroy(m.gameObject);
        activeMaterials.Clear();

        var inventory = PlayerStateMachine.Instance.inventory;
        foreach (var mat in item.materials)
        {
            inventory.slots.TryGetValue(mat.item.itemcode, out int have);
            var entry = Instantiate(materialPrefab, materialListParent);
            entry.Setup(mat.item, mat.amount, have);
            activeMaterials.Add(entry);
        }

        foreach (var slot in activeSlots)
            slot.SetSelected(slot.Item == item);

        UpdateCraftButton();
    }

    private void UpdateCraftButton()
    {
        craftButton.interactable = selectedItem != null &&
            selectedItem.CanCraft(PlayerStateMachine.Instance.inventory);
    }

    private void OnCraftClicked()
    {
        if (selectedItem == null) return;

        // 제작에 실패하면 재료가 되돌려지므로 화면 상태도 바뀌지 않는다.
        // 실패 사유는 인벤토리의 무게·슬롯 초과 안내가 표시한다.
        if (!selectedItem.Craft(PlayerStateMachine.Instance.inventory)) return;

        SelectItem(selectedItem);
    }
}
