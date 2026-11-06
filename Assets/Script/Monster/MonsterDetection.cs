using MonsterStates;
using UnityEngine;

public class MonsterDetection : CharacterSystem<MonsterStateMachine>
{
    IntervalTimer Timer = new();
    bool running;
    int lostFrames;
    const int lostThreshold = 20;         // 2초 유예

    public MonsterDetection(MonsterStateMachine owner) : base(owner) { }
    protected override float interval => 0.1f;

    public override void Start() { running = true; lostFrames = 0; Timer.Reset(); }
    public override void Stop() => running = false;

    public override void Tick()
    {
        if (!running || !Timer.Timer(interval)) return;

        var status = owner.status;
        Collider[] hitPlayers = Physics.OverlapBox(owner.transform.position, status.detectHalfExtents, Quaternion.identity, owner.PlayerLayer);

        if (hitPlayers.Length > 0)
        {
            // 플레이어를 찾음!
            lostFrames = 0;
            owner.SetTarget(hitPlayers[0].GetComponent<PlayerStateMachine>());
            var target = owner.Targetplayer;

            // 전투범위 안 + 빛 → 즉시 감지 / 탐지범위 안 → 게이지 누적
            float dist = Vector3.Distance(owner.transform.position, target.transform.position);
            bool instantDetect = target.status.currentbrighten > 0f
                && dist <= status.battleHalfExtents.x;

            if (instantDetect)
            {
                status.detection_gauge = MonsterStateMachine.MaxDetectionGauge;
            }
            else
            {
                float awareness = CalculateSoundAwareness(target) + CalculateLightAwareness(target);
                status.detection_gauge += awareness * status.detectionSpeed * 0.1f;

                if (awareness <= 0)
                    DecayDetectionGauge();
            }

            // 감지 게이지 초과 시 Chase 전환
            if (status.detection_gauge >= MonsterStateMachine.MaxDetectionGauge)
            {
                if (owner.ActiveState is MonsterStates.Idle || owner.ActiveState is MonsterStates.Move || owner.ActiveState is MonsterStates.Return)
                {
                    owner.ChangeState<Chase>();
                }
            }
        }
        else
        {
            // 탐지 범위 밖: 게이지를 서서히 감소시킨다
            DecayDetectionGauge();

            if (owner.Targetplayer != null)
            {
                // 순찰 중이면 멈추고 경계
                if (lostFrames == 0 && owner.ActiveState is MonsterStates.Move)
                    owner.ChangeState<MonsterStates.Idle>();

                lostFrames++;
                if (lostFrames >= lostThreshold)
                {
                    lostFrames = 0;
                    owner.LoseTarget();
                }
            }
        }
    }

    // 탐지 게이지를 진정도만큼 감소시킨다. 하한은 0. (MonsterStateMachine에서 옮김)
    void DecayDetectionGauge()
    {
        owner.status.detection_gauge = Mathf.Max(0f, owner.status.detection_gauge - owner.status.recovery * 0.1f);
    }

    float CalculateSoundAwareness(PlayerStateMachine player)
    {
        float dist = Vector3.Distance(owner.transform.position, player.transform.position);
        float noise = player.status.currentnoise;

        if (Physics.Linecast(owner.transform.position, player.transform.position, owner.ObstacleMask))
        {
            noise *= 0.2f;
        }

        return noise / Mathf.Max(dist, 1f);
    }
    float CalculateLightAwareness(PlayerStateMachine player)
    {
        if (Physics.Linecast(owner.transform.position, player.transform.position, owner.ObstacleMask))
            return 0f;

        float dist = Vector3.Distance(owner.transform.position, player.transform.position);
        return player.status.currentbrighten / Mathf.Max(dist, 1f);
    }
}
