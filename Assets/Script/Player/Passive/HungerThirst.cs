using System.Collections;
using UnityEngine;

public class HungerThirst : PassiveState
{
    private float hungerAccum = 0f;
    private float thirstAccum = 0f;
    private Coroutine subscribeCoroutine;

    public HungerThirst(PlayerStateMachine player) : base(player) { }

    public override void Enter()
    {
        subscribeCoroutine = player.StartCoroutine(WaitAndSubscribe());
    }

    public override void Exit()
    {
        if (subscribeCoroutine != null) { player.StopCoroutine(subscribeCoroutine); subscribeCoroutine = null; }
        if (GameManager.Instance != null) GameManager.Instance.ChangeTime -= OnMinuteTick;
    }

    private IEnumerator WaitAndSubscribe()
    {
        yield return new WaitUntil(() => GameManager.Instance != null);
        GameManager.Instance.ChangeTime += OnMinuteTick;
        subscribeCoroutine = null;
    }

    private void OnMinuteTick(int hour, int minute)
    {
        hungerAccum += player.status.HungerDecayRate;
        thirstAccum += player.status.ThirstDecayRate;

        int hungerDelta = (int)hungerAccum;
        int thirstDelta = (int)thirstAccum;

        if (hungerDelta > 0) { player.status.Hungry -= hungerDelta; hungerAccum -= hungerDelta; }
        if (thirstDelta > 0) { player.status.Water  -= thirstDelta; thirstAccum -= thirstDelta; }
    }

    protected override void OnTick() { }
}
