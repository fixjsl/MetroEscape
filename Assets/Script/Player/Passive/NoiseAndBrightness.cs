using UnityEngine;
using PlayerStates;

public class NoiseAndBrightness : CharacterSystem<PlayerStateMachine>
{
    // Flashlight가 나중에 EquipLight로 바꿔도 따라가도록 equip에서 바로 읽음
    public Light light => owner.equip.CurrentLight;

    [Header("소음 수치")]
    private float noiseIdle;
    private float noiseWalk;
    private float noiseSprint;
    private float noiseCrouch;
    public NoiseAndBrightness(PlayerStateMachine player) : base(player)  
    {
        RefreshNoiseStats();
    }
    public override void Start()
    {
        owner.status.ChangeNoiseStat += RefreshNoiseStats;
    }

    public override void Stop()
    {
        owner.status.ChangeNoiseStat -= RefreshNoiseStats;
    }
    public override void Tick()
    {
        UpdateNoise();
        UpdateBrighten();
    }

    private void RefreshNoiseStats()
    {
        noiseIdle = owner.status.noiseIdle;
        noiseWalk = owner.status.noiseWalk;
        noiseSprint = owner.status.noiseSprint;
        noiseCrouch = owner.status.noiseCrouch;
    }
    private void UpdateNoise()
    {
        if (owner.input.IsCrouch)
        {
            owner.status.currentnoise = noiseCrouch;
            return;
        }

        if (owner.ActiveState is Move)
        {
            owner.status.currentnoise = owner.input.IsSprint ? noiseSprint : noiseWalk;
        }
        else
        {
            owner.status.currentnoise = noiseIdle;
        }

    }
    private void UpdateBrighten()
    {
        owner.status.currentbrighten = owner.equip.IsLightOn ? 1f : 0f;
    }
}
