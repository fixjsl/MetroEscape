using System.Collections.Generic;
using UnityEngine;

public enum StateType
{
    None, idle, Dodge, Attack, interact,Parry, crouch
}
[RequireComponent(typeof(BoxCollider))]
public class PlayerStateMachine : SingletonBehaviour<PlayerStateMachine>, IEffectTarget, IDamageable
{
    public PlayerInput input;
    public PlayerEquipment equip;
    public PlayerCombat combat;
    public PlayerInteract interact;
    //IEffectTarget
    public StatModifierSet Modifiers => status.Modifiers;
    //IDamageable (이미 죽었으면 무시 → OnDie 중복 방지)
    public void Heal(float amount)
    {
        if (status.Hp <= 0) return;
        status.Hp += amount;
    }
    public void TakeDamage(float amount)
    {
        if (status.Hp <= 0) return;
        status.Hp -= amount;
    }

    public readonly StateMachine<PlayerState> fsm = new();
    //플레이어 객체 및 상태
    public Rigidbody Rb { get; private set; }
    public Animator animator{  get; private set; }
    public BoxCollider Col { get; private set; }
    public Vector3 StandSize { get; private set; }
    public Vector3 StandCenter { get; private set; }
    public float crouchHeight = 1f;
    public PlayerStatus status = new PlayerStatus();
    public Inventory inventory;

    public Transform weaponSlot;
    [SerializeField] private AttackHitbox attackHitbox;
    [SerializeField] private Flashlight flashlight;

    [Header("SFX")]
    [SerializeField] public AudioClip footstepWalk;
    [SerializeField] public AudioClip footstepSprint;
    [SerializeField] public AudioClip dodgeSound;
    [SerializeField] public AudioClip hitSound;
    [SerializeField] public AudioClip deathSound;

    //현재 플레이어의 상태
    public PlayerState ActiveState => fsm.Current;
    public List<CharacterSystem<PlayerStateMachine>> Passive { get; private set; } = new();
    public bool isMovementLocked { get; private set; }
    public void SetMovementLocked(bool locked) => isMovementLocked = locked;
    public void PlaySFX(AudioClip clip) { if (clip != null) GameManager.Instance.audio.PlaySFX(clip); }

    //파리 관련 상태
    public bool isParrying => ActiveState is Parry parry && parry.IsInParryWindow;

    [SerializeField] private ItemBase startItem;
    [SerializeField] private int startItemCount = 1;
    public EffectRunner effects;

    //
    public void stateInit()
    {
        // 판단이 필요한 키만 여기서 결정 (나머지 키는 PlayerInput 안에서 처리)
        input.AttackPressed += () => { if (equip.CurrentWeapon != null) input.SetBuffer(StateType.Attack); };
        input.GuardPressed += () => { if (equip.CurrentWeapon != null) input.SetBuffer(StateType.Parry); };
        input.LightPressed += equip.ToggleLight;
        input.Enable();
        foreach (var s in StateFactory.CreateAll<PlayerState>(this)) fsm.Register(s);
        status.OnDie += () => ChangeState<Die>();

        Passive.Add(new NoiseAndBrightness(this));
        Passive.Add(new HungerThirst(this));

        fsm.Start<Idle>();
    }



    protected override void Awake() {
        base.Awake();
        inventory = new Inventory(status);
        status.curMaxCarryWeight = status.maxCarryWeight;
        if (startItem != null)
            inventory.AddItem(startItem, startItemCount, countAsGathered: false);
        Rb = GetComponent<Rigidbody>();
        Col = GetComponent<BoxCollider>();
        StandSize = Col.size;
        StandCenter = Col.center;
        animator = GetComponent<Animator>();
        animator.SetLayerWeight(1, 0f);
        attackHitbox.Init(this);

        input = new();
        combat = new(attackHitbox);
        interact = new PlayerInteract(this.transform);
        equip = new(animator, weaponSlot, attackHitbox);
        equip.EquipLight(GetComponentInChildren<Light>());

        status.Init();   // 스탯 초기화

        effects = new EffectRunner(this);

        stateInit();
        InputBindings.Init(input.Actions);
    }
    // 시스템 시작은 Start에서 (GameManager 등 다른 오브젝트의 Awake가 끝난 뒤)
    void Start()
    {
        foreach (var s in Passive) s.Start();
    }
    protected override void OnDestroy()
    {
        foreach (var s in Passive) s.Stop();
        base.OnDestroy();
    }
    void Update()
    {
        if (flashlight != null) flashlight.UpdateShaderGlobals();
        input.Tick();
        effects.Tick(Time.deltaTime);
        CheckStateChange();

        combat.Tick();
        foreach (var s in Passive) s.Tick();

        ActiveState?.LogicUpdate();
    }

    [SerializeField] private float fallMultiplier = 3f;

    void FixedUpdate()
    {
        ActiveState?.PhysicalUpdate();

        if (Rb.linearVelocity.y < 0)
            Rb.AddForce(Vector3.down * fallMultiplier, ForceMode.Acceleration);
    }
    //현재 상태에서 다른 상태로 전환 하는 함수
    public void ChangeState<T>() where T : PlayerState
    {
        input.ClearBuffer();
        fsm.ChangeState<T>();
    }
    public bool CheckStateChange()
    {
        if (isMovementLocked) return false;

        // 캔슬 윈도우: canChanged 무관하게 Dodge/Parry 즉시 전환
        if (combat.isInCancelWindow && input.Buffer != StateType.None)
        {
            if (input.Buffer == StateType.Dodge || input.Buffer == StateType.Parry)
            {
                combat.CloseAllAttackWindows();
                BufferState();
                return true;
            }
        }

        // 콤보 윈도우: Attack 버퍼 → 재진입 없이 DoCombo 호출
        if (combat.isInComboWindow && input.Buffer == StateType.Attack)
        {
            combat.CloseAllAttackWindows();
            (ActiveState as Attack)?.DoCombo();
            input.ConsumeBuffer(StateType.Attack);
            return true;
        }

        if (ActiveState.canChanged)
        {

            if (input.IsCrouch && ActiveState is not Crouch)
            {
                ChangeState<Crouch>();
                return true;
            }

            if(ActiveState is Parry && input.IsGuard)
            {
                ChangeState<Guard>();
                return true ;
            }

            if (input.Buffer != StateType.None)
            {
                BufferState();
                return true;
            }
            if((ActiveState is Guard || ActiveState is Parry) && !input.IsGuard)
            {
                ChangeState<Idle>();
                return true ;
            }
            if (ActiveState is Crouch && input.IsCrouch) return false;
            if (input.MoveInput != 0f)
            {
                if (ActiveState is not Move) ChangeState<Move>();
                return true;
            }
            if (ActiveState is not Idle)
            {
                ChangeState<Idle>();
                return true;
            }
        }
        return false;
    }

    public void BufferState()
    {
        var buffered = input.Buffer;   // ChangeState가 버퍼를 비우므로 미리 저장
        switch (buffered)
        {
            case StateType.Attack:
                {
                    if (!fsm.Get<Attack>().CanEnter()) break;

                    MonsterStateMachine execTarget = null;
                    float executionRange = equip.CurrentWeapon.status.executionRange;

                    // 스턴 처형 (기존)
                    Collider[] stunHits = Physics.OverlapSphere(transform.position,
                        executionRange, 1 << LayerCache.Stun);
                    if (stunHits.Length > 0)
                    {
                        execTarget = stunHits[0].GetComponentInParent<MonsterStateMachine>();
                    }

                    // detection_gauge 50 이하 처형
                    if (execTarget == null)
                    {
                        Collider[] monsterHits = Physics.OverlapSphere(transform.position,
                            executionRange, 1 << LayerCache.Monster);
                        foreach (var col in monsterHits)
                        {
                            var m = col.GetComponentInParent<MonsterStateMachine>();
                            if (m != null && m.CanBeExecuted()) { execTarget = m; break; }
                        }
                    }

                    if (execTarget != null) 
                    {
                        var execution = fsm.Get<Execution>() as Execution;
                        execution.setTarget(execTarget);
                        ChangeState<Execution>();
                    }
                    else ChangeState<Attack>();
                    break;
                }
            case StateType.Parry: if (fsm.Get<Parry>().CanEnter()) ChangeState<Parry>(); break;
            case StateType.Dodge:
                {
                    if (fsm.Get<Dodge>().CanEnter())
                    {
                        ChangeState<Dodge>();
                    }
                    else ChangeState<Idle>();
                    break;
                }
            case StateType.interact: ChangeState<Interact>(); break;
            default: ChangeState<Idle>(); break;
        }
        input.ConsumeBuffer(buffered);
    }

    //input 활성화
    public void OnEnable()
    {
        input?.Enable();
    }
    public void OnDisable()
    {
        input?.Disable();
    }


    //이벤트 이벤트 호출 함수
    public void OnHit(float Damage)
    {
        if (ActiveState.isBlock)
        {
            ActiveState?.HandleDamage(Damage);
            return;
        }
        ChangeState<Hit>();
        ActiveState?.HandleDamage(Damage);
    }


    //애니메이션 회전
    void OnAnimatorMove()
    {
        Rb.MoveRotation(Rb.rotation * animator.deltaRotation);
    }
    // 일반 클래스는 Unity 메시지를 못 받으므로 여기서 넘겨줌
    void OnAnimatorIK(int layerIndex) => equip.OnAnimatorIK(layerIndex);

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        var weapon = equip?.CurrentWeapon;
        if (weapon == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, weapon.status.executionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, weapon.status.attackRange);
    }
#endif
}
