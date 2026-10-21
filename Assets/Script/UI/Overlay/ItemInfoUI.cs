using UnityEngine;
using TMPro;

public class ItemInfoUI : SingletonBehaviour<ItemInfoUI>
{

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemTypeText;
    [SerializeField] private TMP_Text itemWeightText;
    [SerializeField] private TMP_Text fuelText;
    [SerializeField] private TMP_Text descriptionText; 

    protected override void Awake()
    {
        base.Awake();
        panel.SetActive(false);
    }

    public void Show(ItemBase item, Vector2 screenPos)
    {
        itemNameText.text = item.name;
        itemTypeText.text = item.itemType.ToString();
        itemWeightText.text = $"무게: {item.weight}";
        if (fuelText != null) fuelText.gameObject.SetActive(item.canBurnAsFuel);
        if (descriptionText != null)
        {
            string effect = item.GetEffectDescription();
            string full = string.IsNullOrEmpty(effect)
                ? item.description
                : string.IsNullOrEmpty(item.description) ? effect : effect + "\n" + item.description;
            descriptionText.text = full;
            descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(full));
        }
        ((RectTransform)panel.transform).position = screenPos + new Vector2(15f, 15f);
        panel.SetActive(true);
    }

    public void Show(string label, Vector2 screenPos)
    {
        if (descriptionText != null) descriptionText.gameObject.SetActive(false);
        itemNameText.text = label;
        itemTypeText.text = "";
        itemWeightText.text = "";
        if (fuelText != null) fuelText.gameObject.SetActive(false);
        ((RectTransform)panel.transform).position = screenPos + new Vector2(15f, 15f);
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
