using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BossStates;
using MonsterStates;

public class BossStateMachine : MonsterStateMachine
{
    [Header("Special Attack")]
    [SerializeField] public float SpecialCooldown = 15f;
    [SerializeField, Range(0f, 1f)] public float SpecialTriggerChance = 0.4f;

    [Header("Return")]
    [SerializeField] public float ReturnSpeed = 12f;

    [Header("Trigger")]
    [SerializeField] private GameObject startTriggerObject;

    [Header("UI")]
    [SerializeField] private BossUI bossUI;

    public readonly int specialHash = Animator.StringToHash("bossSpecial");
    public readonly int firstAttackHash = Animator.StringToHash("bossFirstAttack");

    private float specialTimer;
    public bool IsSpecialReady => specialTimer <= 0f;
    public void ResetSpecialCooldown() => specialTimer = SpecialCooldown;

    public bool IsFirstAttack { get; set; } = true;
    public bool PreserveBattleTimer { get; set; } = false;

    // ──────────────────────────────────────────────
    // BossStartTrigger / BossCombatZone에서 호출

    public void OnPlayerEnterZone(PlayerStateMachine player)
    {
        if (Targetplayer != null) return;

        SetTarget(player);
        status.detection_gauge = 100f;
        IsFirstAttack = true;

        bossUI?.Init(status);

        if (ActiveState is BossIdle || ActiveState is BossReturn)
            ChangeState<BossChase>();
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
        if (Corpse != null) Corpse.enabled = false;
    }

    protected override void Update()
    {
        base.Update();
        if (specialTimer > 0f)
            specialTimer -= Time.deltaTime;
    }

    protected override void OnEnable() { }
    protected override void OnDisable() { }

    public override void stateInit()
    {
        foreach (var s in StateFactory.CreateAll<MonsterState>(this, t => t.IsSubclassOf(typeof(BossState)))) fsm.Register(s);
        fsm.Start<BossIdle>();
    }

    public override void OnHit(float Damage, float stunStrength)
    {
        bool isAttacking = ActiveState is BossAttack || ActiveState.isBlock;
        if (isAttacking)
        {
            ActiveState?.HandleDamage(Damage);
            return;
        }

        var hitState = fsm.Get<BossHit>() as BossHit;
        hitState.SetHitDuration(stunStrength);
        ChangeState<BossHit>();
        ActiveState?.HandleDamage(Damage);
    }
    public override void OnExeHit(float Damage)
{
    ActiveState?.HandleDamage(Damage);
    if (status.Hp <= 0) return;
    OnPlayerEnterZone(PlayerStateMachine.Instance);
} 

    public override void ChangeStun() => ChangeState<BossStun>();

    public override void LoseTarget()
    {
        SetTarget(null);
        ChangeState<BossReturn>();
    }

    protected override void OnDieHandler() => ChangeState<BossDie>();

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
