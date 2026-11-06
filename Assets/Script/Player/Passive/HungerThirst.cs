using UnityEngine;

public class HungerThirst : CharacterSystem<PlayerStateMachine>
{
    private float hungerAccum = 0f;
    private float thirstAccum = 0f;
    private Coroutine subscribeCoroutine;

    public HungerThirst(PlayerStateMachine player):base(player){ }

    public override void Start() => GameManager.Instance.clock.ChangeTime += OnMinuteTick;

    public override void Stop()
    {
        if (GameManager.Instance != null) GameManager.Instance.clock.ChangeTime -= OnMinuteTick;
    }


    private void OnMinuteTick(int hour, int minute)
    {
        hungerAccum += owner.status.HungerDecayRate;
        thirstAccum += owner.status.ThirstDecayRate;

        int hungerDelta = (int)hungerAccum;
        int thirstDelta = (int)thirstAccum;

        if (hungerDelta > 0) { owner.status.Hungry -= hungerDelta; hungerAccum -= hungerDelta; }
        if (thirstDelta > 0) { owner.status.Water  -= thirstDelta; thirstAccum -= thirstDelta; }
    }

    public override void Tick() { }
}
