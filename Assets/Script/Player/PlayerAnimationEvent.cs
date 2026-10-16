using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    public PlayerStateMachine player;

    private void Awake()
    {
        player = GetComponent<PlayerStateMachine>();
    }
    public void OnAnimationFinished()
    {
        player.ActiveState?.OnAnimationFinished();
    }

    public void OnTurnAnimationFinished()
    {
        player.ActiveState?.OnTurnAnimationFinished();
    }
    public void OncanCombo()
    {
        player.ActiveState?.OncanCombo();
    }
    public void OnAnimationFinishedAt(int attackIndex)
    {
        player.ActiveState?.OnAnimationFinishedAt(attackIndex);
    }
    public void OnFootstep()
    {
        var clip = (player.isSprint && !player.isTired) ? player.footstepSprint : player.footstepWalk;
        player.PlaySFX(clip);
    }

    public void OnAttackCollider() => player.OnAttackColider();
    public void OffAttackCollider() => player.OffAttackColider();
    public void OnEndInvincible()
    {
        player.gameObject.layer = Layercache.Player;
    }
    public void OnDamage()
    {
        if(player.ActiveState is Execution)
        {
            ((Execution)player.ActiveState).OnDamage();
        }
    }
}
