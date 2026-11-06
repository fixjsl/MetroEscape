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
    public override int ReturnAnim => MonsterAnim.sprint;
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
        status.detection_gauge = MaxDetectionGauge;
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

}
