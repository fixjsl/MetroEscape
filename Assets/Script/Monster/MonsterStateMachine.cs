
using UnityEngine;
using MonsterStates;


public class MonsterStateMachine : MonoBehaviour
{
    public readonly StateMachine<MonsterState> fsm = new();
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] protected LayerMask playerLayer;
    public LayerMask ObstacleMask => obstacleMask;   // MonsterDetection이 읽음
    public LayerMask PlayerLayer => playerLayer;     // MonsterDetection이 읽음
    private MonsterDetection detection;              // UseDetection이 false면 null (보스)
    // UI 등 바깥에서 구독 (MonsterUIBinder)
    public event System.Action Hidden;      // HideUI() — 풀로 돌아갈 때
    public event System.Action Respawned;   // CycleRespawn() 끝
    public PlayerStateMachine Targetplayer {  get; private set; }
    public MonsterState ActiveState => fsm.Current;
    public Rigidbody Rb { get; private set; }
    public Animator animator { get; private set; }
    private Collider _col;
    public MonsterStatus status;
    public const float MaxDetectionGauge = 100f;   // 감지 게이지 최대(= 발견)
    public const float ExecutableGauge = 50f;      // 이 값 이하면 처형 가능
    public int[] AttackHashes => status.AttackHashes;
    [SerializeField] private LootTable lootTable;
    [SerializeField] private ItemPickup itemPickupPrefab;
    [SerializeField] private ContainerObject corpse;
    public ContainerObject Corpse => corpse;

    [SerializeField]
    public Transform spawnpoint;
    [SerializeField] private float hitAnimLength = 0.5f;
    public float HitAnimLength => hitAnimLength;
    [SerializeField] private float stunAnimLength = 1f;
    public float StunAnimLength => stunAnimLength;
    [SerializeField] private Collider attackCollider; 
     public bool IsReturningFromAttack { get; set; }
    public Collider AttackCollider => attackCollider;
    // 훅영역
    public bool PreserveBattleTimer { get; set; } = false;
    protected virtual bool UseDetection => true; //탐지
    public virtual bool Patrols => true; // 순찰
    public virtual float ReturnSpeed => status.speed;//속도
    public virtual int ReturnAnim => MonsterAnim.move; //애니메이션
    public virtual void OnReturnArrived() => ChangeState<MonsterStates.Idle>();
    public virtual void OnReachedBattleRange() => ChangeState<MonsterStates.Battle>();
    public virtual void ChooseAttack() => ChangeState<MonsterStates.Attack>();
    public virtual int PickAttackAnim() => AttackHashes[Random.Range(0, AttackHashes.Length)];
    public virtual float BattleChargeSpeed => status.chasespeed;
    public virtual float ApproachAnimSpeed => 0.7f;
    public virtual bool KeepsBattleTimer => false;
    public virtual bool ScalesStunAnim => true;
    public virtual bool CountsAsKill => true;
    public virtual bool RespawnsOnCycle => true;
    public virtual bool IgnoresHitReaction => ActiveState.isBlock;
    public virtual void OnAggro() {
        status.detection_gauge = MaxDetectionGauge;
        if (Targetplayer == null) SetTarget(PlayerStateMachine.Instance);
    }
    public virtual void stateInit()
    {
        foreach (var s in StateFactory.CreateAll<MonsterState>(this, t => t.Namespace == "MonsterStates")) fsm.Register(s);
        fsm.Start<MonsterStates.Idle>();
    }
    protected virtual void Awake()
    {

        status = Instantiate(status);
        IsReturningFromAttack = false;
        status.Hp = status.Maxhp;
        Rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        _col = GetComponent<Collider>();
        if (corpse != null) corpse.enabled = false;
        status.OnDie += OnDieHandler;
        status.OnDie += SpawnLoot;
        if (UseDetection) detection = new MonsterDetection(this);
        stateInit();

        MonsterManager.Register(this);
    }
    public bool IsFrozen { get; private set; }

    public void Freeze()
    {
        IsFrozen = true;
        Rb.isKinematic = true;
    }

    public void Unfreeze()
    {
        IsFrozen = false;
        Rb.isKinematic = false;
    }

    protected virtual void Update()
    {
        if (!IsFrozen) ActiveState?.LogicUpdate();
        detection?.Tick();   // 예전 코루틴처럼 Freeze 중에도 돎
    }
    protected virtual void FixedUpdate()
    {
        if (!IsFrozen) ActiveState?.PhysicalUpdate();
    }
    public void ChangeState<T>() where T : MonsterState
    {
        fsm.ChangeState<T>();
    }
    public virtual void OnHit(float Damage, float stunStrength)
    {

        OnAggro();
        if (IgnoresHitReaction)
        {
            ActiveState?.HandleDamage(Damage);
            return;
        }
        var hitState = fsm.Get<MonsterStates.Hit>() as MonsterStates.Hit;
        hitState.SetHitDuration(stunStrength);
        ChangeState<MonsterStates.Hit>();
        ActiveState?.HandleDamage(Damage);
    }
    public virtual void OnExeHit(float Damage)
    {
        ActiveState?.HandleDamage(Damage);
        if (status.Hp <= 0) return;
        if (Targetplayer == null)
            SetTarget(PlayerStateMachine.Instance);
        ChangeState<Battle>();
    }
    public void HideUI()
    {
        Hidden?.Invoke();
    }
    protected virtual void OnEnable()
    {
        StartDetection();
    }
    protected virtual void OnDisable()
    {
        StopDetection();
    }
    public void StartDetection() => detection?.Start();
    public void StopDetection() => detection?.Stop();

    public bool IsInBattleRange()
    {
        if (Targetplayer == null) return false;
        Vector3 delta = Targetplayer.transform.position - transform.position;
        return Mathf.Abs(delta.x) <= status.battleHalfExtents.x &&
               Mathf.Abs(delta.y) <= status.battleHalfExtents.y;
    }

    public bool CanBeExecuted() => status.detection_gauge <= ExecutableGauge;

    public void SetTarget(PlayerStateMachine target)
    {
        Targetplayer = target;
    }

    public virtual void LoseTarget()
    {
        Targetplayer = null;
        ChangeState<Return>();
    }

    public virtual void ChangeStun()
    {
        ChangeState<Stun>();
    }
    public bool NeedTurn(float targetx)
    {
        float dirToPlayer = targetx - transform.position.x;
        // 내가 바라보는 방향
        float myDir = Vector3.Dot(transform.forward, Vector3.right);
        return (dirToPlayer > 0 && myDir < 0) || (dirToPlayer < 0 && myDir > 0);
    }
    public void FaceTowardX(float targetx)
    {
        float dirToPlayer = targetx - transform.position.x;
        float targetY = dirToPlayer > 0 ? 90f : 270f;
        Rb.rotation = Quaternion.Euler(0, targetY, 0);
    }
    public void CycleRespawn()
    {
        gameObject.SetActive(true);
        enabled = true;

        gameObject.layer = LayerCache.Monster;
        if (_col != null) { _col.isTrigger = false; _col.enabled = true; }
        if (AttackCollider != null)
        {
            AttackCollider.gameObject.layer = LayerCache.MonsterHitbox;
            AttackCollider.enabled = false;
        }

        if (corpse != null) corpse.enabled = false;

        IsFrozen = false;
        Rb.isKinematic = false;
        animator.applyRootMotion = true;
        animator.speed = 1f;

        status.Hp = status.Maxhp;
        status.detection_gauge = 0f;
        IsReturningFromAttack = false;
        Targetplayer = null;

        if (spawnpoint != null)
            Rb.position = spawnpoint.position;
        Rb.linearVelocity = Vector3.zero;

        ChangeState<MonsterStates.Idle>();
        Respawned?.Invoke();
    }

    protected virtual void OnDieHandler()
    {
        ChangeState<MonsterStates.Die>();
    }
    public bool TurnToward(float targetX, int moveAnim)
    {
        if (NeedTurn(targetX))
        {
            animator.CrossFade(MonsterAnim.moveTurn, 0.01f);
            return true;
        }
        animator.CrossFade(moveAnim, 0.01f);
        return false;
    }
    public void FinishTurn(float targetX, int moveAnim)
    {
        FaceTowardX(targetX);
        animator.CrossFade(moveAnim, 0.0001f);
    }
    private void SpawnLoot()
    {
        if (lootTable == null || itemPickupPrefab == null) return;
        var drops = lootTable.Roll();
        for (int i = 0; i < drops.Count; i++)
        {
            var pos = transform.position + Vector3.up * 0.5f + Vector3.right * (i * 0.5f);
            var pickup = Instantiate(itemPickupPrefab, pos, Quaternion.identity);
            pickup.Setup(drops[i].item, drops[i].count);
        }
    }

    void OnAnimatorMove()
    {
        Rb.MoveRotation(Rb.rotation * animator.deltaRotation);
    }
    private void OnDestroy()
    {
        MonsterManager.Unregister(this);
    }

}
