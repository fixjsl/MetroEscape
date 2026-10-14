using NUnit.Framework;
using System;
using System.Collections.Generic;
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
             
             hp = Mathf.Clamp(value, 0, Maxhp);
            ChangeHP?.Invoke(hp);
             if (hp <= 0) OnDie?.Invoke();
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

            if (stamina <= 0) StaminaEmpty?.Invoke();
        }
    }
    public float currentspeed;
    public float walkspeed;
    public float crunchspeed;
    public float sprintspeed;
    public float crunchsprintspeed;

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
    private List<StatModifier> Effect = new List<StatModifier>();
    public void AddModifier(StatModifier modifier)
    {
        Effect.Add(modifier);
    }
    public void RemoveModifier(StatModifier modifier)
    {
        Effect.Remove(modifier);
    }
    public void ClearModifier(StatModifier modifier)
    {
        Effect.Clear();
    }
    float GetMultiplier(StatType type)
    {
        float up = 1f, down = 1f;
        foreach (var m in Effect)
        {
            if (m.Stat != type) continue;
            if (m.multiplier > 1f) up   = Mathf.Max(up,   m.multiplier);
            else                   down = Mathf.Min(down, m.multiplier);
        }
        return up * down;
    }


    [Header("Recovery")]
    public float staminaRecoverey;                 // 그대로 (저장된 10/15 유지)
    [Header("Survival")]
    public float hungerDecreasePerMinute = 0.1f;   // 그대로
    public float waterDecreasePerMinute = 0.3f;
    public float StaminaRecoveryRate => staminaRecoverey * GetMultiplier(StatType.Stamina);
    public float HungerDecayRate => hungerDecreasePerMinute * GetMultiplier(StatType.Hungry);
    public float ThirstDecayRate => waterDecreasePerMinute * GetMultiplier(StatType.Thirst);

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
    public void UseStamina(float amount)
    {
        Stamina -= amount; 
    }
}
