using UnityEngine;

public class WeaponPickup : InteractableBase
{
    private Weapon weapon;

    public override string InteractMessage => Prompt("줍기");

    public void SetWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        if (weapon == null) return;
        PlayerStateMachine.Instance.equip.EquipWeapon(weapon);
        GetComponent<Collider>().enabled = false;
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnTrigger(TutorialTriggerType.CombatStart);
    }
}
