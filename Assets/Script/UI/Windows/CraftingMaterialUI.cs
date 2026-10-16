using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CraftingMaterialUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text countText;

    private ItemBase _item;
    private string _labelOverride;

    public void Setup(ItemBase item, int required, int current)
    {
        _item = item;
        _labelOverride = null;
        if (icon != null) icon.sprite = item.icon;
        if (countText != null)
        {
            countText.text = $"{current} / {required}";
            countText.color = current >= required ? Color.white : Color.red;
        }
    }

    public void Setup(Sprite iconSprite, int required, int current, string label = "타는 물질")
    {
        _item = null;
        _labelOverride = label;
        if (icon != null) icon.sprite = iconSprite;
        if (countText != null)
        {
            countText.text = $"{current} / {required}";
            countText.color = current >= required ? Color.white : Color.red;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemInfoUI.Instance == null) return;
        Vector2 pos = eventData.position;
        if (_item != null)
            ItemInfoUI.Instance.Show(_item, pos);
        else if (_labelOverride != null)
            ItemInfoUI.Instance.Show(_labelOverride, pos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemInfoUI.Instance != null)
            ItemInfoUI.Instance.Hide();
    }
}
