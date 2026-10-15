using System.Collections;
using UnityEngine;

public class Die : PlayerState
{
    public Die(PlayerStateMachine stateMachine) : base(stateMachine) {
       
    }

    public override void Enter()
    {
        player.animator.CrossFade(player.die, 0.001f);
        player.PlaySFX(player.deathSound);
        canChanged = false;
        player.Rb.linearVelocity = Vector3.zero;
        player.Rb.isKinematic = true;
        player.gameObject.layer = Layercache.Die;
        player.OnDisable();
        player.inventory.LoseOnDeath(player.status.curdeathItemLossRate);
        player.effects.ClearOnDeath();
        player.status.Chance--;     
        if (player.status.Chance >0)
            player.StartCoroutine(RespawnRoutine());
        else
            GameManager.Instance.GameOver();
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSecondsRealtime(2f);

        
        player.status.Hp = player.status.Maxhp;

        CycleManager.Instance.ExecuteCycleReset();

        player.gameObject.layer = Layercache.Player;
        player.Rb.isKinematic = false;
        player.OnEnable();
        ContinueGame();
        player.ChangeState<Idle>();
    }

    public override void Exit() { }

    public override void LogicUpdate() { }

    public override void PhysicalUpdate() { }

    public void ContinueGame()
    {
        canChanged = true;
    }
}
