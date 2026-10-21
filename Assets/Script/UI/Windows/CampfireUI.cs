using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CampfireUI : UIPanel<CampfireUI>
{

    [SerializeField] private CraftingMaterialUI fuelSlot;
    [SerializeField] private CraftingMaterialUI lighterSlot;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    private Campfire currentCampfire;

    protected override void Awake()
    {
        base.Awake();
        root.SetActive(false);
        actionButton.onClick.AddListener(OnActionClicked);
    }

    
    public void Open(Campfire campfire)
    {
        currentCampfire = campfire;
        Open();
    }
    protected override void OnOpened() => Refresh(currentCampfire);

    protected override void OnClosed()
    {
        currentCampfire = null;
    }


    public void Refresh(Campfire campfire)
    {
        if (!IsOpen) return;
        currentCampfire = campfire;
        var inv = PlayerStateMachine.Instance.inventory;

        if (campfire.IsBurning)
        {
            inv.slots.TryGetValue(campfire.RawMeat.itemcode, out int haveMeat);
            fuelSlot.Setup(campfire.RawMeat, 1, haveMeat);
            lighterSlot.gameObject.SetActive(false);
            actionButtonText.text = "굽기";
            actionButton.interactable = haveMeat >= 1;
        }
        else
        {
            int haveFuel = Campfire.CountFuel(inv);
            fuelSlot.Setup(campfire.FuelIcon, campfire.RequiredFuelCount, haveFuel);

            lighterSlot.gameObject.SetActive(campfire.LighterItem != null);
            if (campfire.LighterItem != null)
            {
                inv.slots.TryGetValue(campfire.LighterItem.itemcode, out int haveLighter);
                lighterSlot.Setup(campfire.LighterItem, 1, haveLighter);
                actionButton.interactable = haveFuel >= campfire.RequiredFuelCount && haveLighter >= 1;
            }
            else
            {
                actionButton.interactable = haveFuel >= campfire.RequiredFuelCount;
            }

            actionButtonText.text = "불 피우기";
        }
    }

    private void OnActionClicked()
    {
        if (currentCampfire == null) return;
        if (currentCampfire.IsBurning)
            currentCampfire.CookMeat();
        else
            currentCampfire.LightFire();
    }
}
