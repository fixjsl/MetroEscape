using UnityEngine;
public class Weapon : MonoBehaviour
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

    public void SetPlayer(PlayerStateMachine stateMachine)
    {
        player = stateMachine;
    }
}
