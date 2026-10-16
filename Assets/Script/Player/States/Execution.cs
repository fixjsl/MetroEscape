using UnityEngine;

public class Execution : PlayerState
{
    private MonsterStateMachine target;
    private float damage;
    public Execution(PlayerStateMachine player) : base(player)
    {
        canChanged = false;
    }

    public void setTarget(MonsterStateMachine monster)
    {
        target = monster;
    }
    public override void Enter()
    {
        canChanged = false;
        damage = player.currentWeapon.status.attack * player.currentWeapon.status.execution_m;
        player.animator.CrossFade(player.execution, 0.15f);
        player.animator.CrossFade(player.idle, 0.15f, 0);
        player.gameObject.layer = Layercache.Dodge;
        player.Rb.linearVelocity = Vector3.zero;

        if (target != null)
        {
            float offset = target.status.minSeparation;
            Vector3 execPos = player.transform.position + player.transform.forward * offset;
            execPos.y = target.Rb.position.y;
            target.Rb.position = execPos;
            target.Rb.linearVelocity = Vector3.zero;
            target.Freeze();
        }
    }
    public override void Exit()
    {
        player.gameObject.layer = Layercache.Player;
        if (target != null)
            target.Unfreeze();
    }

    public void OnDamage()
    {
        if (target != null)
        {
            target.OnExeHit(damage);
        }
    }


}
