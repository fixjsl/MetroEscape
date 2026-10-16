using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ContainerSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Transform infoAnchor;

    private System.Action onLeftClick;
    private System.Action<Vector2> onRightClick;
    private ItemBase currentItem;

    public void Setup(ItemBase item, int count, System.Action onLeft, System.Action<Vector2> onRight = null)
    {
        currentItem = item;
        icon.sprite = item.icon;
        countText.text = count.ToString();
        onLeftClick = onLeft;
        onRightClick = onRight;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            onLeftClick?.Invoke();
        else if (eventData.button == PointerEventData.InputButton.Right)
            onRightClick?.Invoke(GetSlotRightScreenPos());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null)
            ItemInfoUI.Instance.Show(currentItem, GetSlotRightScreenPos());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemInfoUI.Instance.Hide();
    }

    private Vector2 GetSlotRightScreenPos()
    {
        return RectTransformUtility.WorldToScreenPoint(null, infoAnchor.position);
    }
}
