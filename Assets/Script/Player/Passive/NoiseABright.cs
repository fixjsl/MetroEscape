using UnityEngine;

public class NoiseABright : PassiveState
{
    public Light light;

    [Header("���� ��ġ")]
    private float noiseIdle;
    private float noiseWalk;
    private float noiseSprint;
    private float noiseCrouch;
    public NoiseABright(PlayerStateMachine player) : base(player)  
    {
        interval = 0f;
        light = player.currentLight;
        RefreshNoiseStats();
    }
    public override void Enter()
    {
        player.status.ChangeNoiseStat += RefreshNoiseStats;
        base.Enter();
    }

    public override void Exit()
    {
        player.status.ChangeNoiseStat -= RefreshNoiseStats;
        base.Exit();
    }
    protected override void OnTick()
    {
        UpdateNoise();
        UpdateBrighten();
    }

    private void RefreshNoiseStats()
    {
        noiseIdle = player.status.noiseIdle;
        noiseWalk = player.status.noiseWalk;
        noiseSprint = player.status.noiseSprint;
        noiseCrouch = player.status.noiseCrouch;
    }
    private void UpdateNoise()
    {
        if (player.isCrunch)
        {
            player.status.currentnoise = noiseCrouch;
            return;
        }

        if (player.ActiveState is Move)
        {
            player.status.currentnoise = player.isSprint ? noiseSprint : noiseWalk;
        }
        else
        {
            player.status.currentnoise = noiseIdle;
        }

    }
    private void UpdateBrighten()
    {
        player.status.currentbrighten = player.IsLightOn ? 1f : 0f;
    }
}
