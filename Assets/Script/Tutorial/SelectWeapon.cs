using UnityEngine;
using UnityEngine.UI;

public class SelectWeapon : MonoBehaviour
{
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private Button selectButton;

    void Awake()
    {
        selectButton.onClick.AddListener(OnSelect);
    }

    void OnSelect()
    {
        var weaponObj = Instantiate(weaponPrefab);
        if (weaponObj.TryGetComponent<Weapon>(out var weapon))
            PlayerStateMachine.Instance.EquipWeapon(weapon);
        else
            Destroy(weaponObj);

        GameStartFlow.Instance.OnWeaponSelected();
    }
}
