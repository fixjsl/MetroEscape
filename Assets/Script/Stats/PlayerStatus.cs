using System;
using UnityEngine;
[System.Serializable]
public class PlayerStatus
{
    [Header("Default Stat")]
    public int Chance = 3;
    public string Name;
    public int Maxhp;
    private float hp;
    public float Hp { 
        get => hp;
        set {
            bool wasAlive = hp > 0;
            hp = Mathf.Clamp(value, 0, Maxhp);
            ChangeHP?.Invoke(hp);
            if (wasAlive && hp <= 0) OnDie?.Invoke();
        } 
    }
    public int MaxStamina;

    public int curMaxStamina;
    private float stamina;
    public float Stamina
    {
        get => stamina; 
        set
        {
            stamina = Mathf.Clamp(value, 0, curMaxStamina);
           ChangeStamina?.Invoke(stamina);

            if (stamina <= 0) { isTired = true; StaminaEmpty?.Invoke(); }
            else if (isTired && stamina >= MaxStamina * TiredRecoverRatio) isTired = false;
        }
    }
    // 스태미나가 0이 되면 true, 최대치의 30%까지 회복하면 false (PlayerStateMachine에서 옮김)
    public bool isTired { get; private set; }
    const float TiredRecoverRatio = 0.3f;   // 지침은 최대치의 30%까지 회복하면 풀림
    public void TickStamina(bool sprinting, float dt) {
        if (!sprinting && Stamina >= curMaxStamina) return;
        Stamina += sprinting ? -SprintCost * dt : StaminaRecoveryRate * dt; 
    }
    public float currentspeed;
    public float walkspeed;
    [UnityEngine.Serialization.FormerlySerializedAs("crunchspeed")]
    public float crouchspeed;
    public float sprintspeed;
    [UnityEngine.Serialization.FormerlySerializedAs("crunchsprintspeed")]
    public float crouchsprintspeed;

    [Header("Player Status")]
    public float parryduration;
    public readonly int MaxHungry = 100;
    private int hungry; 
    public int Hungry
    {
        get => hungry; 
        set
        {
           hungry = Mathf.Clamp(value, 0, MaxHungry);
           ChangeHungry?.Invoke(hungry);
           if(hungry <= 30)
            {
                 curMaxCarryWeight = maxCarryWeight * 0.4f; 
            }
            else
            {
                curMaxCarryWeight = maxCarryWeight;
            }
        }
    }
    public readonly int MaxWater = 100;
    private int water;
    public int Water
    {
        get => water;
        set
        {
            water = Mathf.Clamp(value, 0 , MaxWater);
            ChangeWater?.Invoke(water);
            if(water<= 30)
            {
                curMaxStamina = (int)(MaxStamina * 0.4f);
            }
            else curMaxStamina = MaxStamina;
        }   
    }
    public float DodgeCooldown;
    [Header("Stamina Cost")]
    public float parryCost;
    public float DodgeCost;
    public float SprintCost;
    [Header("Player Hidden")]
    public float currentnoise;
    public float currentbrighten;
    [Header("Noise")]
    [SerializeField] private float _noiseIdle;
    [SerializeField] private float _noiseCrouch;
    [SerializeField] private float _noiseWalk;
    [SerializeField] private float _noiseSprint;
    public float noiseIdle
    {
        get => _noiseIdle;
        set { _noiseIdle = value; ChangeNoiseStat?.Invoke(); }
    }
    public float noiseCrouch
    {
        get => _noiseCrouch;
        set { _noiseCrouch = value; ChangeNoiseStat?.Invoke(); }
    }
    public float noiseWalk
    {
        get => _noiseWalk;
        set { _noiseWalk = value; ChangeNoiseStat?.Invoke(); }
    }
    public float noiseSprint
    {
        get => _noiseSprint;
        set { _noiseSprint = value; ChangeNoiseStat?.Invoke(); }
    }
    //ItemMethod
    public StatModifierSet Modifiers { get; } = new StatModifierSet();


    [Header("Recovery")]
    // 예전 이름 staminaRecoverey로 저장된 값(10/15)을 그대로 읽어옴 — 이게 없으면 0이 돼서 회복이 안 됨
    [UnityEngine.Serialization.FormerlySerializedAs("staminaRecoverey")]
    public float StaminaRecovery;
    [Header("Survival")]
    public float hungerDecreasePerMinute = 0.1f;   // 그대로
    public float waterDecreasePerMinute = 0.3f;
    public float StaminaRecoveryRate => StaminaRecovery * Modifiers.GetMultiplier(StatType.Stamina);
    public float HungerDecayRate => hungerDecreasePerMinute * Modifiers.GetMultiplier(StatType.Hungry);
    public float ThirstDecayRate => waterDecreasePerMinute * Modifiers.GetMultiplier(StatType.Thirst);

    public float dodgeSpeed;

    [Header("Inventory Status")]
    public float maxCarryWeight;
    public float curMaxCarryWeight;
    public float maxSlots;
    public float curdeathItemLossRate;
    //Event
    //base Stat event
    public event Action<float> ChangeHP;
    public event Action OnDie;
    public event Action<float> ChangeStamina;
    public event Action StaminaEmpty;
    public event Action<int> ChangeHungry;
    public event Action<int> ChangeWater;
    //Noise stat event
    public event Action ChangeNoiseStat;
    // ✨ 시작 스탯 채우기 (PlayerStateMachine.Awake의 "스탯 초기화" 5줄을 옮김)
    public void Init()
    {
        curMaxStamina = MaxStamina;
        Hp      = Maxhp;
        Stamina = MaxStamina;
        Hungry  = MaxHungry;
        Water   = MaxWater;
    }
    public void UseStamina(float amount)
    {
        Stamina -= amount; 
    }
}
