using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CraftingUI : UIPanel<CraftingUI>
{
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
    private RecipeSO selectedRecipe;
    private readonly List<CraftingSlotUI> activeSlots = new();
    private readonly List<CraftingMaterialUI> activeMaterials = new();

    protected override void Awake()
    {
        base.Awake();
        root.SetActive(false);
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


    protected override void OnOpened()
    {
        SelectTab(0);
    }

    protected override void OnClosed()
    {
        selectedRecipe = null;
    }


    private void SelectTab(int index)
    {
        currentTab = index;
        selectedRecipe = null;
        detailPanel.SetActive(false);
        RefreshList();
    }

    private void RefreshList()
    {
        foreach (var slot in activeSlots) Destroy(slot.gameObject);
        activeSlots.Clear();

        var type = TabTypes[currentTab];

        foreach (var item in ItemManager.Recipes)
        {
            if (item == null || item.result == null) continue;
            bool typeMatch = item.result.itemType == type ||
                              (type == ItemType.Consumable && item.result.itemType == ItemType.InteractObject);
            if (!typeMatch) continue;


            var slot = Instantiate(craftingSlotPrefab, recipeListParent);
            var captured = item;
            slot.Setup(item.result, () => SelectRecipe(captured));
            activeSlots.Add(slot);
        }
    }

    private void SelectRecipe(RecipeSO recipe)
    {
        selectedRecipe = recipe;
        detailPanel.SetActive(true);

        if (selectedIcon != null) selectedIcon.sprite = recipe.result.icon;
        if (selectedNameText != null) selectedNameText.text = recipe.result.name;
        if (selectedDescText != null) selectedDescText.text = recipe.result.description;

        foreach (var m in activeMaterials) Destroy(m.gameObject);
        activeMaterials.Clear();

        var inventory = PlayerStateMachine.Instance.inventory;
        foreach (var mat in recipe.ingredients)
        {
            inventory.slots.TryGetValue(mat.item.itemcode, out int have);
            var entry = Instantiate(materialPrefab, materialListParent);
            entry.Setup(mat.item, mat.amount, have);
            activeMaterials.Add(entry);
        }

        foreach (var slot in activeSlots)
            slot.SetSelected(slot.Item == recipe.result);

        UpdateCraftButton();
    }

    private void UpdateCraftButton()
    {
        craftButton.interactable = selectedRecipe != null &&
            selectedRecipe.CanCraft(PlayerStateMachine.Instance.inventory);
    }

    private void OnCraftClicked()
    {
        if (selectedRecipe == null) return;

        // 제작에 실패하면 재료가 되돌려지므로 화면 상태도 바뀌지 않는다.
        // 실패 사유는 인벤토리의 무게·슬롯 초과 안내가 표시한다.
        if (!selectedRecipe.Craft(PlayerStateMachine.Instance.inventory)) return;

        SelectRecipe(selectedRecipe);
    }
}
