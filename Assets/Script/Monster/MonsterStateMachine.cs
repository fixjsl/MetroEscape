using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using MonsterStates;
using System.Collections;


public class MonsterStateMachine : MonoBehaviour
{
    public readonly StateMachine<MonsterState> fsm = new();
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] protected LayerMask playerLayer;
    public Coroutine DetectCorutine { get; private set; }
    public PlayerStateMachine Targetplayer {  get; private set; }
    public MonsterState ActiveState => fsm.Current;
    public MonsterState PassiveState { get; private set; }
    public Rigidbody Rb { get; private set; }
    public Animator animator { get; private set; }
    private Collider _col;
    public MonsterStatus status;
    public readonly int idle = Animator.StringToHash("idle");
    public readonly int move = Animator.StringToHash("move");
    public readonly int sprint = Animator.StringToHash("chase");
    public readonly int battle = Animator.StringToHash("battle");
    public readonly int hit = Animator.StringToHash("hit");
    public readonly int die = Animator.StringToHash("die");
    public readonly int moveTurn = Animator.StringToHash("moveTurn");
    public readonly int stun = Animator.StringToHash("stun");
    public int[] AttackHashes => status.AttackHashes;
    [SerializeField] private LootTable lootTable;
    [SerializeField] private ItemPickup itemPickupPrefab;
    [SerializeField] private ContainerObject corpse;
    public ContainerObject Corpse => corpse;

    [Header("��������Ʈ �����ٶ�")]
    [SerializeField]
    public Transform spawnpoint;
    [SerializeField] private MonsterUI monsterUIPrefab;
    [SerializeField] private Transform hpUIPoint;
    [SerializeField] private Transform detectionUIPoint;
    private MonsterUI monsterUI;
    [SerializeField] private float hitAnimLength = 0.5f;
    public float HitAnimLength => hitAnimLength;
    [SerializeField] private float stunAnimLength = 1f;
    public float StunAnimLength => stunAnimLength;
    [SerializeField] private Collider attackCollider; // �ν����� �Ҵ��
     public bool IsReturningFromAttack { get; set; }
    public Collider AttackCollider => attackCollider; // �ܺο��� �б� ����
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
        stateInit();

        if (monsterUIPrefab != null)
        {
            monsterUI = Instantiate(monsterUIPrefab);
            monsterUI.Init(this, hpUIPoint, detectionUIPoint);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
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
        status.detection_gauge = 100f;
        if (Targetplayer == null) SetTarget(PlayerStateMachine.Instance);

        if (ActiveState.isBlock)
        {
            ActiveState?.HandleDamage(Damage);
            return;
        }
        var hitState = fsm.Get<MonsterStates.Hit>() as MonsterStates.Hit;
        hitState.SetHitduration(stunStrength);
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
    protected virtual void OnEnable()
    {
        StartDetection();
    }
    protected virtual void OnDisable()
    {
        StopDetection();
    }
    public void StartDetection()
    {
        StopDetection(); // �ߺ� ���� ����
        DetectCorutine = StartCoroutine(Detect());
    }

    public void StopDetection()
    {
        if (DetectCorutine != null)
        {
            StopCoroutine(DetectCorutine);
            DetectCorutine = null;
        }
    }
    IEnumerator Detect()
    {
        WaitForSeconds wait = YeildCache.GetIntervals(0.1f);
        int lostFrames = 0;
        const int lostThreshold = 20; // 2초 유예
        while (true)
        {
            Collider[] hitPlayers = Physics.OverlapBox(transform.position, status.detectHalfExtents, Quaternion.identity, playerLayer);

            if (hitPlayers.Length > 0)
            {
                // �÷��̾ ã��!
                lostFrames = 0;
                Targetplayer = hitPlayers[0].GetComponent<PlayerStateMachine>();

                // 2. ���⼭ ���� �츮�� ®�� ��/���� ��� �Լ��� �����ϴ�.
                // 전투범위 안 + 빛 → 즉시 감지 / 탐지범위 안 → 게이지 누적
                float dist = Vector3.Distance(transform.position, Targetplayer.transform.position);
                bool instantDetect = Targetplayer.status.currentbrighten > 0f
                    && dist <= status.battleHalfExtents.x;

                if (instantDetect)
                {
                    status.detection_gauge = 100f;
                }
                else
                {
                    float awareness = CalculateSoundAwareness(Targetplayer) + CalculateLightAwareness(Targetplayer);
                    status.detection_gauge += awareness * status.detectionSpeed * 0.1f;

                    if (awareness <= 0)
                        DecayDetectionGauge();
                }

                // 감지 게이지 초과 시 Chase 전환
                if (status.detection_gauge >= 100f)
                {
                    if (ActiveState is MonsterStates.Idle || ActiveState is MonsterStates.Move || ActiveState is MonsterStates.Return)
                    {
                        ChangeState<Chase>();
                    }
                }

            }
            else
            {
                // 탐지 범위 밖: 게이지를 서서히 감소시킨다
                DecayDetectionGauge();

                if (Targetplayer != null)
                {
                    // 순찰 중이면 멈추고 경계
                    if (lostFrames == 0 && ActiveState is MonsterStates.Move)
                        ChangeState<MonsterStates.Idle>();

                    lostFrames++;
                    if (lostFrames >= lostThreshold)
                    {
                        lostFrames = 0;
                        LoseTarget();
                    }
                }
            }

            yield return wait;
           }

        }
    // 탐지 게이지를 진정도만큼 감소시킨다. 하한은 0.
    void DecayDetectionGauge()
    {
        status.detection_gauge = Mathf.Max(0f, status.detection_gauge - status.recovery * 0.1f);
    }

    float CalculateSoundAwareness(PlayerStateMachine player)
    {
        float dist = Vector3.Distance(transform.position, player.transform.position);
        float noise = player.status.currentnoise;

        // ��ֹ��� �ִ��� üũ (Linecast)
        if (Physics.Linecast(transform.position, player.transform.position, obstacleMask))
        {
            noise *= 0.2f; // ���� ������ ���� 80% ����
        }

        return noise / Mathf.Max(dist, 1f);
    }
    float CalculateLightAwareness(PlayerStateMachine player)
    {
        if (Physics.Linecast(transform.position, player.transform.position, obstacleMask))
            return 0f;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        return player.status.currentbrighten / Mathf.Max(dist, 1f);
    }
   

    public bool IsInBattleRange()
    {
        if (Targetplayer == null) return false;
        Vector3 delta = Targetplayer.transform.position - transform.position;
        return Mathf.Abs(delta.x) <= status.battleHalfExtents.x &&
               Mathf.Abs(delta.y) <= status.battleHalfExtents.y;
    }

    public bool CanBeExecuted() => status.detection_gauge <= 50f;

    public void SetTarget(PlayerStateMachine target)
    {
        Targetplayer = target;
    }

    public virtual void LoseTarget()
    {
        Targetplayer = null;
        // � ���¿� �ֵ� Ÿ���� ������ ����
        ChangeState<Return>();
    }

    public virtual void ChangeStun()
    {
        ChangeState<Stun>();
    }

    public void CycleRespawn()
    {
        gameObject.SetActive(true);
        enabled = true;

        gameObject.layer = Layercache.Monster;
        if (_col != null) { _col.isTrigger = false; _col.enabled = true; }
        if (AttackCollider != null)
        {
            AttackCollider.gameObject.layer = Layercache.MonsterHitbox;
            AttackCollider.enabled = false;
        }

        if (corpse != null) corpse.enabled = false;

        if (monsterUI != null) monsterUI.ResetForRespawn();

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
    }

    protected virtual void OnDieHandler()
    {
        ChangeState<MonsterStates.Die>();
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

#if UNITY_EDITOR
    [SerializeField] private bool debugMode = false;

    void Start()
    {
        if (debugMode) StartCoroutine(DebugStatusCoroutine());
    }

    protected virtual IEnumerator DebugStatusCoroutine()
    {
        var wait = new WaitForSeconds(0.5f);
        while (true)
        {
            yield return wait;

            string targetName = Targetplayer != null ? Targetplayer.name : "없음";
            float dist = Targetplayer != null
                ? Vector3.Distance(transform.position, Targetplayer.transform.position)
                : -1f;

            float brighten = Targetplayer != null ? Targetplayer.status.currentbrighten : -1f;
            bool hasLOS = Targetplayer != null && !Physics.Linecast(transform.position, Targetplayer.transform.position, obstacleMask);
            Debug.Log(
                $"[MONSTER DEBUG] {gameObject.name}\n" +
                $"  ActiveState     : {ActiveState?.GetType().Name ?? "null"}\n" +
                $"  HP              : {status.Hp:F0} / {status.Maxhp}\n" +
                $"  DetectionGauge  : {status.detection_gauge:F1} / 100\n" +
                $"  Target          : {targetName}\n" +
                $"  TargetDist      : {(dist >= 0 ? dist.ToString("F1") : "N/A")}\n" +
                $"  CurBrighten     : {(brighten >= 0 ? brighten.ToString("F3") : "N/A")}\n" +
                $"  HasLOS          : {(Targetplayer != null ? hasLOS.ToString() : "N/A")}\n" +
                $"  DetectRange     : {status.detectHalfExtents}\n" +
                $"  BattleRange     : {status.battleHalfExtents}\n" +
                $"  --- 물리 ---\n" +
                $"  Velocity        : {Rb.linearVelocity}\n" +
                $"  Position        : {Rb.position}\n" +
                $"  IsKinematic     : {Rb.isKinematic}\n" +
                $"  Constraints     : {Rb.constraints}\n" +
                $"  Rotation(Y)     : {Rb.rotation.eulerAngles.y:F1}"
            );
        }
    }
    void OnDrawGizmos()
    {
        // 감지 범위 (노랑)
        Gizmos.color = Color.yellow;
        if (status != null) Gizmos.DrawWireCube(transform.position, status.detectHalfExtents * 2f);

        // 전투 범위 (빨강)
        Gizmos.color = Color.red;
        if (status != null) Gizmos.DrawWireCube(transform.position, status.battleHalfExtents * 2f);

        // 공격 범위 (마젠타)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, status != null ? status.atk_range : 0f);
    }
#endif

}
