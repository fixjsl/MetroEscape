using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image background;
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.4f, 0.6f, 0.4f, 1f);

    private ItemBase item;
    private System.Action onSelect;

    public ItemBase Item => item;

    public void Setup(ItemBase itemBase, System.Action onClick)
    {
        item = itemBase;
        onSelect = onClick;
        if (icon != null) icon.sprite = itemBase.icon;
        if (nameText != null) nameText.text = itemBase.name;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (background != null)
            background.color = selected ? selectedColor : normalColor;
    }

    public void OnClick() => onSelect?.Invoke();
}
