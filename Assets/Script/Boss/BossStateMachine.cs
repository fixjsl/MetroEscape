using System.Collections;
using UnityEngine;
using BossStates;
using MonsterStates;
using UnityEngine.Serialization;

public class BossStateMachine : MonsterStateMachine
{
    [Header("Special Attack")]
    [SerializeField] public float SpecialCooldown = 15f;
    [SerializeField, Range(0f, 1f)] public float SpecialTriggerChance = 0.4f;

    [Header("Return")]
    [SerializeField][FormerlySerializedAs("ReturnSpeed")] private float returnSpeed = 12f;

    [Header("Trigger")]
    [SerializeField] private GameObject startTriggerObject;

    [Header("UI")]
    [SerializeField] private BossUI bossUI;

    public override float ApproachAnimSpeed => 1;
    public override float BattleChargeSpeed => status.speed;
    public override void ChooseAttack()
    {
        if (IsSpecialReady && Random.value < SpecialTriggerChance)
            ChangeState<BossSpecial>();
        else
            ChangeState<MonsterStates.Attack>();
    }
    public override bool CountsAsKill => false;
    public override bool IgnoresHitReaction => ActiveState.isBlock || ActiveState is MonsterStates.Attack;
    public override bool KeepsBattleTimer => true;
    public override bool Patrols => false;
    protected override bool UseDetection => false;
    public override bool RespawnsOnCycle => false;
    public override int ReturnAnim => sprint;
    public override float ReturnSpeed => returnSpeed;
    public override bool ScalesStunAnim => false;
    public override void OnAggro() { }
    public override void OnReachedBattleRange() { ChangeState<MonsterStates.Attack>(); }
    public override void OnReturnArrived() { ActivateStartTrigger();ChangeState<MonsterStates.Idle>(); }
    public override int PickAttackAnim()
    {
        if (IsFirstAttack)
        {
            IsFirstAttack = false;
            return firstAttackHash;
        }
        else
            return base.PickAttackAnim();
    }
    public readonly int specialHash = Animator.StringToHash("bossSpecial");
    public readonly int firstAttackHash = Animator.StringToHash("bossFirstAttack");

    private float specialTimer;
    public bool IsSpecialReady => specialTimer <= 0f;
    public void ResetSpecialCooldown() => specialTimer = SpecialCooldown;

    public bool IsFirstAttack { get; set; } = true;

    // ──────────────────────────────────────────────
    // BossStartTrigger / BossCombatZone에서 호출

    public void OnPlayerEnterZone(PlayerStateMachine player)
    {
        if (Targetplayer != null) return;

        SetTarget(player);
        status.detection_gauge = 100f;
        IsFirstAttack = true;

        bossUI?.Init(status);

        if (ActiveState is MonsterStates.Idle || ActiveState is Return)
            ChangeState<MonsterStates.Chase>();
    }

    public void OnPlayerExitZone()
    {
        if (Targetplayer == null) return;
        OnPlayerEscapedZone();
    }

    public void OnPlayerEscapedZone()
    {
        status.Hp = status.Maxhp;
        status.detection_gauge = 0f;
        IsFirstAttack = true;
        PreserveBattleTimer = false;

        bossUI?.Hide();

        LoseTarget();
    }

    public void ActivateStartTrigger() => startTriggerObject?.SetActive(true);

    // ──────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        specialTimer = SpecialCooldown;
    }

    protected override void Update()
    {
        base.Update();
        if (specialTimer > 0f)
            specialTimer -= Time.deltaTime;
    }
    public override void stateInit()
    {
        foreach (var s in StateFactory.CreateAll<MonsterState>(this, t => t.Namespace == "MonsterStates" || t.IsSubclassOf(typeof(BossState)))) fsm.Register(s);
        fsm.Start<MonsterStates.Idle>();
    }
    public override void OnExeHit(float Damage)
    {
        ActiveState?.HandleDamage(Damage);
        if (status.Hp <= 0) return;
        OnPlayerEnterZone(PlayerStateMachine.Instance);
    } 


#if UNITY_EDITOR
    protected override IEnumerator DebugStatusCoroutine()
    {
        var wait = new WaitForSeconds(0.5f);
        while (true)
        {
            yield return wait;

            string targetName = Targetplayer != null ? Targetplayer.name : "없음";
            float dist = Targetplayer != null
                ? Vector3.Distance(transform.position, Targetplayer.transform.position)
                : -1f;

            Debug.Log(
                $"[BOSS DEBUG] {gameObject.name}\n" +
                $"  ActiveState     : {ActiveState?.GetType().Name ?? "null"}\n" +
                $"  HP              : {status.Hp:F0} / {status.Maxhp}\n" +
                $"  Target          : {targetName}\n" +
                $"  TargetDist      : {(dist >= 0 ? dist.ToString("F1") : "N/A")}\n" +
                $"  BattleRange     : {status.battleHalfExtents}\n" +
                $"  IsFirstAttack   : {IsFirstAttack}\n" +
                $"  IsSpecialReady  : {IsSpecialReady}\n" +
                $"  --- 물리 ---\n" +
                $"  Velocity        : {Rb.linearVelocity}\n" +
                $"  Position        : {Rb.position}\n" +
                $"  Rotation(Y)     : {Rb.rotation.eulerAngles.y:F1}"
            );
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (status != null) Gizmos.DrawWireCube(transform.position, status.battleHalfExtents * 2f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, status != null ? status.atk_range : 0f);
    }
#endif
}
