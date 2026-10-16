using UnityEngine;
using System.Collections.Generic;

public class AttackHitbox : MonoBehaviour
{
    private PlayerStateMachine player;
    private BoxCollider col;
    private HashSet<MonsterStateMachine> hitTargets = new HashSet<MonsterStateMachine>();

    public void Init(PlayerStateMachine stateMachine)
    {
        player = stateMachine;
        col = GetComponent<BoxCollider>();
        col.enabled = false;
    }

    public void SetupRange(float attackRange)
    {
        col.center = new Vector3((attackRange * 10) / 2f, col.center.y, col.center.z);
        col.size = new Vector3(attackRange*10, col.size.y, col.size.z);
    }

    public void EnableHitbox()
    {
        hitTargets.Clear();
        col.enabled = true;
    }

    public void DisableHitbox()
    {
        col.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var monster = other.GetComponentInParent<MonsterStateMachine>();
        if (monster == null) return;
        if (!hitTargets.Add(monster)) return;
        player.currentWeapon.OnHit(monster);
    }
}
