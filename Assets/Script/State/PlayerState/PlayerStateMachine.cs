using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public enum StateType
{
    None, idle, Dodge, Attack, interact,Parry, crunch
}
[RequireComponent(typeof(BoxCollider))]
public class PlayerStateMachine : MonoBehaviour, IEffectTarget, IDamageable
{
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


    public static PlayerStateMachine Instance { get; private set; }

    public readonly StateMachine<PlayerState> fsm = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;
    //플레이어 키입력
    private InputSystem_Actions action;
    //플레이어 객체 및 상태
    public Rigidbody Rb { get; private set; }
    public Animator animator{  get; private set; }
    public BoxCollider Col { get; private set; }
    public Vector3 StandSize { get; private set; }
    public Vector3 StandCenter { get; private set; }
    public float crouchHeight = 1f;
    public PlayerStatus status = new PlayerStatus();
    public Inventory inventory;
    public Weapon currentWeapon;
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
    public float MoveInput;
    float lastMoveInput;
    public Light currentLight {get; private set; }
    public bool IsLightOn { get; private set; } = true;
    public event System.Action<bool> OnLightToggle;
    public PlayerState ActiveState => fsm.Current;
    public List<PlayerState> PassiveStates { get; private set; } = new List<PlayerState>();
    public bool isGuard { get; private set; }
    public bool isSprint {  get; private set; }
    public bool isCrunch { get; private set; }
    public bool isTired { get; private set; }
    public bool isMovementLocked { get; private set; }
    public void SetMovementLocked(bool locked) => isMovementLocked = locked;
    //애니메이션 상태 해시
    public readonly int idle  = Animator.StringToHash("idle");
    public readonly int move = Animator.StringToHash("move");
    public readonly int moveTurn = Animator.StringToHash("moveTurn");
    public readonly int hit  = Animator.StringToHash("hit");
    public readonly int die = Animator.StringToHash("die");
    public readonly int tired = Animator.StringToHash("tired");
    public readonly int[] attackHashes = {
    Animator.StringToHash("attack1"),
    Animator.StringToHash("attack2"),
    Animator.StringToHash("attack3")
    };
    public readonly int execution = Animator.StringToHash("execution");
    public readonly int sprint = Animator.StringToHash("sprint");
    public readonly int sprintTurn = Animator.StringToHash("sprintTurn");
    public readonly int incrunch = Animator.StringToHash("incrunch");
    public readonly int outcrunch = Animator.StringToHash("outcrunch");
    public readonly int crunchturn = Animator.StringToHash("crunchturn");
    public readonly int crunch = Animator.StringToHash("crunch");
    public readonly int crunchmove = Animator.StringToHash("crunchmove");
    public readonly int parrysucess = Animator.StringToHash("parrysucess");
    public readonly int parrying = Animator.StringToHash("parrying");
    public readonly int guard = Animator.StringToHash("guard");
    public readonly int dodge = Animator.StringToHash("dodge");
    public event Action ParrySuccess;
    public void InvokeParrySuccess() => ParrySuccess?.Invoke();
    public void PlaySFX(AudioClip clip) { if (clip != null) GameManager.Instance.PlaySFX(clip); }
    //플레이어 버퍼입력
    public StateType bufferinput { get; private set; }
    public float buffertime { get; private set; } = 0.2f;
    public TimeManager bufferTimer = new TimeManager();
    // 공격 캔슬/콤보 윈도우
    public bool isInCancelWindow { get; private set; }
    public bool isInComboWindow { get; private set; }
    private bool postAnimWindowActive;
    private readonly TimeManager postAnimTimer = new();
    private const float PostAnimComboTime = 0.5f;

    //파리 관련 상태
    public bool isParrying => ActiveState is Parry parry && parry.IsInParryWindow;

    [SerializeField] private ItemBase startItem;
    [SerializeField] private int startItemCount = 1;
    public EffectRunner effects;
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    public IInteractable nearbyInteractable => GetClosest();
    public void SetInteractable(IInteractable interactable)
    {
    if (!nearbyInteractables.Contains(interactable))
        nearbyInteractables.Add(interactable);
    }
    public void ClearInteractable(IInteractable interactable)
    {
        nearbyInteractables.Remove(interactable);
    }
    private IInteractable GetClosest()
    {
        if (nearbyInteractables.Count == 0) return null;
        return nearbyInteractables
            .OrderBy(i => (((MonoBehaviour)i).transform.position - transform.position).sqrMagnitude)
            .First();
    }
    //
    public void stateInit()
    {
        action = new InputSystem_Actions();
        action.PlayerAction.Attack.performed += _ => { if (currentWeapon != null) SetBuffer(StateType.Attack); };
        action.PlayerAction.Dodge.performed += _ => SetBuffer(StateType.Dodge);
        action.PlayerAction.Interact.performed += _ => SetBuffer(StateType.interact);
        action.PlayerAction.LightTogle.performed += _ => {
            bool next = currentLight == null || !currentLight.enabled;
            if (currentLight != null) currentLight.enabled = next;
            IsLightOn = next;
            OnLightToggle?.Invoke(next);
        };
        action.PlayerAction.Move.performed += ctx => {
            float value = ctx.ReadValue<float>();
            // 반대 방향일 때만 업데이트
            if (lastMoveInput == 0f)
            {
                MoveInput = value;
                lastMoveInput = value;
            }
        };
        action.PlayerAction.Move.canceled += ctx =>
        {
            MoveInput = 0f;
            lastMoveInput = 0f;
        };

        action.PlayerAction.Crouch.performed += _ => isCrunch = !isCrunch;
        action.PlayerAction.Sprint.performed += _ => isSprint = true;
        action.PlayerAction.Guard.performed += _ =>
        {
            isGuard = true;
            if (currentWeapon != null)
            {
                SetBuffer(StateType.Parry);
            }
        };
        action.PlayerAction.Sprint.canceled += _ => isSprint = false;
        action.PlayerAction.Guard.canceled += _ => isGuard = false;
        action.Enable();
        foreach (var s in StateFactory.CreateAll<PlayerState>(this)) fsm.Register(s);
        AddpassiveStat<NoiseABright>();
        AddpassiveStat<HungerThirst>();
        status.OnDie += () => ChangeState<Die>();
        status.StaminaEmpty += () => { isTired = true; };
       
        fsm.Start<Idle>();
    }



    void Awake() {
        if(Instance == null) Instance = this;
        inventory = new Inventory(status);
        status.curMaxCarryWeight = status.maxCarryWeight;
        if (startItem != null)
            inventory.AddItem(startItem, startItemCount, suppressFirstAcquire: true, countAsGathered: false);
        currentLight = GetComponentInChildren<Light>();
        Rb = GetComponent<Rigidbody>();
        Col = GetComponent<BoxCollider>();
        StandSize = Col.size;
        StandCenter = Col.center;
        animator = GetComponent<Animator>();
        animator.SetLayerWeight(1, 0f);
        attackHitbox.Init(this);

        // 스탯 초기화
        status.curMaxStamina = status.MaxStamina;
        status.Hp      = status.Maxhp;
        status.Stamina = status.MaxStamina;
        status.Hungry  = status.MaxHungry;
        status.Water   = status.MaxWater;

        effects = new EffectRunner(this);

        stateInit();
        InputBindings.Init(action);
    }
    void Update()
    {
        if (flashlight != null) flashlight.UpdateShaderGlobals();
        effects.Tick(Time.deltaTime);
        CheckStateChange();
#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (inventory.slots.Count == 0) { Debug.Log("[인벤토리] 비어있음"); }
            else
            {
                foreach (var (code, count) in inventory.slots)
                {
                    string name = ItemManager.itemDB.TryGetValue(code, out var item) ? item.name : $"코드:{code}";
                    Debug.Log($"[인벤토리] {name} x{count}");
                }
            }
        }
#endif

        if (isTired && status.Stamina >= status.MaxStamina * 0.3f)
            isTired = false;

        if (bufferinput != StateType.None)
        {
            if (bufferTimer.Timer(buffertime))
            {
                bufferinput = StateType.None;
            }
        }

        if (postAnimWindowActive)
        {
            if (postAnimTimer.Timer(PostAnimComboTime))
            {
                postAnimWindowActive = false;
                isInComboWindow = false;
            }
        }

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
        bufferinput = StateType.None;
        fsm.ChangeState<T>();
    }
    public bool CheckStateChange()
    {
        if (isMovementLocked) return false;

        // 캔슬 윈도우: canChanged 무관하게 Dodge/Parry 즉시 전환
        if (isInCancelWindow && bufferinput != StateType.None)
        {
            if (bufferinput == StateType.Dodge || bufferinput == StateType.Parry)
            {
                CloseAllAttackWindows();
                BufferState();
                return true;
            }
        }

        // 콤보 윈도우: Attack 버퍼 → 재진입 없이 DoCombo 호출
        if (isInComboWindow && bufferinput == StateType.Attack)
        {
            CloseAllAttackWindows();
            (ActiveState as Attack)?.DoCombo();
            ConsumeBuffer(StateType.Attack);
            return true;
        }

        if (ActiveState.canChanged)
        {

            if (isCrunch && ActiveState is not Crunch)
            {
                ChangeState<Crunch>();
                return true;
            }

            if(ActiveState is Parry && isGuard)
            {
                ChangeState<Guard>();
                return true ;
            }

            if (bufferinput != StateType.None)
            {
                BufferState();
                return true;
            }
            if((ActiveState is Guard || ActiveState is Parry) && !isGuard)
            {
                ChangeState<Idle>();
                return true ;
            }
            if (ActiveState is Crunch && isCrunch) return false;
            if (MoveInput != 0f)
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
        switch (bufferinput)
        {
            case StateType.Attack:
                {
                    if (!fsm.Get<Attack>().CanEnter()) break;

                    MonsterStateMachine execTarget = null;

                    // 스턴 처형 (기존)
                    Collider[] stunHits = Physics.OverlapSphere(transform.position,
                        currentWeapon.status.executionRange, 1 << Layercache.Stun);
                    if (stunHits.Length > 0)
                    {
                        execTarget = stunHits[0].GetComponentInParent<MonsterStateMachine>();
                    }

                    // detection_gauge 50 이하 처형
                    if (execTarget == null)
                    {
                        Collider[] monsterHits = Physics.OverlapSphere(transform.position,
                            currentWeapon.status.executionRange, 1 << Layercache.Monster);
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
        ConsumeBuffer(bufferinput);
    }
    //버퍼입력 함수
    public void SetBuffer(StateType buffertag)
    {
        bufferinput = buffertag;
        bufferTimer.Reset();
    }
    public bool ConsumeBuffer(StateType buffertag)
    {
        if(bufferinput == buffertag)
        {
            bufferinput = StateType.None;
            return true;
        }
        return false;
    }
    // ========== 공격 캔슬/콤보 윈도우 ==========
    public void OpenAttackCancelWindow(bool withCombo)
    {
        isInCancelWindow = true;
        isInComboWindow = withCombo;
    }

    public void CloseAllAttackWindows()
    {
        isInCancelWindow = false;
        isInComboWindow = false;
        postAnimWindowActive = false;
    }

    // Attack.OnAnimationFinished에서 호출
    public void OnAttackAnimFinished(int comboIndex)
    {
        isInCancelWindow = false;
        if (comboIndex >= 2)
        {
            isInComboWindow = false;
            postAnimWindowActive = false;
        }
        else if (isInComboWindow)
        {
            // 1/2타 완료, 0.5초 추가 콤보 유예
            postAnimWindowActive = true;
            postAnimTimer.Reset();
        }
    }

    //PassiveList에 추가하거나 제거
    public void AddpassiveStat<T>() where T : PlayerState
    {
        PlayerState passive = fsm.Get<T>();

        if (PassiveStates.Contains(passive)) return;

        PassiveStates.Add(passive);
        passive.Enter();
    }
    public void RemovepassiveStat<T>() where T : PlayerState
    {
        PlayerState passive = fsm.Get<T>();

        if (PassiveStates.Contains(passive)) {
            passive.Exit();
            PassiveStates.Remove(passive);
        }
    }
    public void ClearpassiveStat()
    {
        foreach(var state in PassiveStates)
        {
            state.Exit();
        }
        PassiveStates.Clear();
    }
    //action활성화

    public void OnEnable()
    {
        action?.Enable();
    }
    public void OnDisable()
    {
        action?.Disable();
    }

#if UNITY_EDITOR
    [SerializeField] private bool debugMode = false;

    void Start()
    {
        if (debugMode) StartCoroutine(DebugStatusCoroutine());
    }

    private IEnumerator DebugStatusCoroutine()
    {
        var wait = new WaitForSeconds(5f);
        while (true)
        {
            yield return wait;

            var info0 = animator.GetCurrentAnimatorStateInfo(0);
            bool inTransition0 = animator.IsInTransition(0);
            string animName0 = inTransition0
                ? $"{GetDebugAnimName(info0)} → {GetDebugAnimName(animator.GetNextAnimatorStateInfo(0))} (전환중)"
                : GetDebugAnimName(info0);

            var info1 = animator.GetCurrentAnimatorStateInfo(1);
            bool inTransition1 = animator.IsInTransition(1);
            float layer1Weight = animator.GetLayerWeight(1);
            string animName1 = layer1Weight == 0f ? "꺼짐" : inTransition1
                ? $"{GetDebugAnimName(info1)} → {GetDebugAnimName(animator.GetNextAnimatorStateInfo(1))} (전환중)"
                : GetDebugAnimName(info1);

            string passiveList = PassiveStates.Count > 0
                ? string.Join(", ", PassiveStates.ConvertAll(s => s.GetType().Name))
                : "없음";

            string interactList = nearbyInteractables.Count > 0
                ? string.Join(", ", nearbyInteractables.ConvertAll(i => ((MonoBehaviour)i).name))
                : "없음";

            Debug.Log(
                $"[DEBUG STATUS]\n" +
                $"  ActiveState  : {ActiveState?.GetType().Name ?? "null"}\n" +
                $"  canChanged   : {ActiveState?.canChanged}\n" +
                $"  Layer0 Anim  : {animName0}  (normalized: {info0.normalizedTime:F2})\n" +
                $"  Layer1 Anim  : {animName1}  (weight: {layer1Weight:F2})\n" +
                $"  PassiveStates: {passiveList}\n" +
                $"  BufferInput  : {bufferinput}\n" +
                $"  MoveInput    : {MoveInput}\n" +
                $"  isGuard / isSprint / isCrunch : {isGuard} / {isSprint} / {isCrunch}\n" +
                $"  NearbyInteractables ({nearbyInteractables.Count}): {interactList}\n" +
                $"  HP           : {status.Hp:F0} / {status.Maxhp}\n" +
                $"  Stamina      : {status.Stamina:F0} / {status.curMaxStamina}\n" +
                $"  Hungry       : {status.Hungry} / {status.MaxHungry}\n" +
                $"  Water        : {status.Water} / {status.MaxWater}"
            );
        }
    }

    private string GetDebugAnimName(AnimatorStateInfo info)
    {
        var map = new Dictionary<int, string>
        {
            { idle,       "idle"       }, { move,       "move"       },
            { moveTurn,   "moveTurn"   }, { hit,        "hit"        },
            { die,        "die"        }, { sprint,     "sprint"     },
            { sprintTurn, "sprintTurn" }, { incrunch,   "incrunch"   },
            { outcrunch,  "outcrunch"  }, { crunchturn, "crunchturn" },
            { crunch, "crunch" }, { crunchmove, "crunchmove" },
            { parrying,   "parrying"   }, { guard,      "guard"      },
            { dodge,      "dodge"      }
        };
        foreach (var kv in map)
            if (info.shortNameHash == kv.Key) return kv.Value;
        for (int i = 0; i < attackHashes.Length; i++)
            if (info.shortNameHash == attackHashes[i]) return $"attack{i + 1}";
        return $"unknown(hash:{info.shortNameHash})";
    }
#endif

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
    public void EquipWeapon(Weapon weapon)
    {
        if (currentWeapon != null)
            Destroy(currentWeapon.gameObject);

        animator.SetLayerWeight(1, 1f);

        currentWeapon = weapon;
        weapon.SetPlayer(this);
        attackHitbox.SetupRange(weapon.status.attackRange);
        if (weapon.status.WeaponAnimations != null)
            animator.runtimeAnimatorController = weapon.status.WeaponAnimations;
        weapon.transform.SetParent(weaponSlot, true);
        if (weapon.primaryGrip != null)
        {
            weapon.transform.rotation = weaponSlot.rotation * Quaternion.Inverse(weapon.primaryGrip.localRotation);
            weapon.transform.position += weaponSlot.position - weapon.transform.TransformPoint(weapon.primaryGrip.localPosition);
        }
    }
    public void OnAttackColider() => attackHitbox.EnableHitbox();
    public void OffAttackColider() => attackHitbox.DisableHitbox();

    public void EquipLight(Light newLight)
    {
        currentLight = newLight;
        (fsm.Get<NoiseABright>() as NoiseABright).light = newLight;
    }
    //애니메이션 회전
    void OnAnimatorMove()
    {
        Rb.MoveRotation(Rb.rotation * animator.deltaRotation);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (currentWeapon != null && currentWeapon.secondaryGrip != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, currentWeapon.secondaryGrip.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, currentWeapon.secondaryGrip.rotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
        }
    }
    // ========== 애니메이션 이벤트 ==========

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (currentWeapon == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, currentWeapon.status.executionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currentWeapon.status.attackRange);
    }
#endif
}
