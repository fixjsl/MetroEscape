using System;
using UnityEngine;

public class PlayerEquipment
{
    private readonly Animator animator;
    private readonly Transform weaponSlot;
    private readonly AttackHitbox attackHitbox;
    public PlayerEquipment(Animator animator, Transform weaponSlot, AttackHitbox attackHitbox) { 
        this.animator = animator;
        this.weaponSlot = weaponSlot;
        this.attackHitbox = attackHitbox;
    }
    public Weapon CurrentWeapon { get; private set; }          // 밖에서 바꾸지 못하게
    public Light CurrentLight { get; private set; }
    public bool IsLightOn { get; private set; } = true;
    public event Action<bool> OnLightToggle;
    public void EquipWeapon(Weapon weapon)
    {
        if (CurrentWeapon != null) UnityEngine.Object.Destroy(CurrentWeapon.gameObject);
        animator.SetLayerWeight(1, 1f);
        CurrentWeapon = weapon;
        attackHitbox.SetupRange(weapon.status.attackRange);
        if (weapon.status.WeaponAnimations != null)
            animator.runtimeAnimatorController = weapon.status.WeaponAnimations;
        weapon.transform.SetParent(weaponSlot, true);
        if (weapon.primaryGrip != null)
        {
            weapon.transform.rotation = weaponSlot.rotation * Quaternion.Inverse(weapon.primaryGrip.localRotation);
            weapon.transform.position += weaponSlot.position - weapon.transform.TransformPoint(weapon.primaryGrip.localPosition);
        }
    }
    public void OnAnimatorIK(int layerIndex)   // PlayerStateMachine.OnAnimatorIK에서 호출
    {
        if (CurrentWeapon != null && CurrentWeapon.secondaryGrip != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, CurrentWeapon.secondaryGrip.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, CurrentWeapon.secondaryGrip.rotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
        }
    }
    public void EquipLight(Light newLight) => CurrentLight = newLight;
    public void ToggleLight()
    {
        bool next = CurrentLight == null || !CurrentLight.enabled;
        if (CurrentLight != null) CurrentLight.enabled = next;
        IsLightOn = next;
        OnLightToggle?.Invoke(next);
    }
}
