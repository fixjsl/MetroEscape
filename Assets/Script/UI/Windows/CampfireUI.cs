using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CampfireUI : MonoBehaviour
{
    public static CampfireUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [SerializeField] private GameObject panel;
    [SerializeField] private CraftingMaterialUI fuelSlot;
    [SerializeField] private CraftingMaterialUI lighterSlot;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    private Campfire currentCampfire;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        actionButton.onClick.AddListener(OnActionClicked);
    }

    public bool IsOpen => panel.activeSelf;

    public void Open(Campfire campfire)
    {
        currentCampfire = campfire;
        panel.SetActive(true);
        Refresh(campfire);
        SetCombatInput(false);
    }

    public void Close()
    {
        panel.SetActive(false);
        currentCampfire = null;
        SetCombatInput(true);
    }

    private void SetCombatInput(bool enabled)
    {
        System.Action<InputAction> toggle = enabled ? a => a.Enable() : a => a.Disable();
        toggle(InputBindings.GetAction("Attack"));
        toggle(InputBindings.GetAction("Dodge"));
        toggle(InputBindings.GetAction("Guard"));
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
