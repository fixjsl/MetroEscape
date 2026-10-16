using UnityEngine;

public class MonsterWeapon : MonoBehaviour
{
    private MonsterStateMachine monster;
    private Collider hitbox;

    private bool hitDealt = false;
    private bool prevHitboxEnabled = false;

    private void Awake()
    {
        monster = GetComponentInParent<MonsterStateMachine>();
        hitbox = monster.AttackCollider as Collider;
        hitbox.enabled = false;
    }

    private void FixedUpdate()
    {
        bool enabled = hitbox != null && hitbox.enabled;
        if (enabled != prevHitboxEnabled)
            Debug.Log($"[MonsterWeapon] {gameObject.name} hitbox enabled={enabled}");
        if (!prevHitboxEnabled && enabled)
            hitDealt = false;
        prevHitboxEnabled = enabled;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[MonsterWeapon] Enter: {other.name} layer={LayerMask.LayerToName(other.gameObject.layer)} hitbox={hitbox != null && hitbox.enabled} hitDealt={hitDealt}");
        if (other.gameObject.layer == Layercache.PlayerHitbox) return;
        if (hitbox == null || !hitbox.enabled || hitDealt) return;
        ProcessHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == Layercache.PlayerHitbox) return;
        if (hitbox == null || !hitbox.enabled || hitDealt) return;
        Debug.Log($"[MonsterWeapon] Stay hit: {other.name}");
        ProcessHit(other);
    }

    private void ProcessHit(Collider other)
    {
        var player = other.GetComponentInParent<PlayerStateMachine>();
        if (player == null) return;
        hitDealt = true;
        if (player.isParrying)
        {
            monster.ChangeStun();
            player.InvokeParrySuccess();
            Debug.Log("ParryingSucess");
            return;
        }
        player.OnHit(monster.status.atk);
    }
}
