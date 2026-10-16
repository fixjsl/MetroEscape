using UnityEngine;
public class Weapon : MonoBehaviour, ICreatable
{
    private PlayerStateMachine player;
    public WeaponStatus status;
    [SerializeField] private ItemBase _iteminfo;
    public ItemBase iteminfo => _iteminfo;
    public Transform primaryGrip;
    public Transform secondaryGrip;

    private void Awake()
    {
        status = Instantiate(status);
    }

    public virtual void OnHit(MonsterStateMachine monster)
    {
        monster.OnHit(status.attack, status.stunStrength);
    }

    public bool canCraft(Inventory inventory)
    {
        foreach (var material in iteminfo.materials)
        {
            if (!inventory.slots.TryGetValue(material.item.itemcode, out int count)) return false;
            if (count < material.amount) return false;
        }
        return true;
    }

    public void Craft(Inventory inventory)
    {
        if (!canCraft(inventory)) return;

        foreach (var material in iteminfo.materials)
            inventory.RemoveItem(material.item, material.amount);
        player.EquipWeapon(this);
    }
    public void SetPlayer(PlayerStateMachine stateMachine)
    {
        player = stateMachine;
    }
}
