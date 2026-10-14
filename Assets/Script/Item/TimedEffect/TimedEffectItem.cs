using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "TimedEffectItem", menuName = "Scriptable Objects/Item/TimedEffectItem")]
public class TimedEffectItem : ItemBase
{
    private void Reset() => itemType = ItemType.Consumable;

    public float duration = 60f;

    [Header("Hunger")]
    public bool modifyHunger;
    public float hungerDecreaseMultiplier = 1f;

    [Header("Thirst")]
    public bool modifyThirst;
    public float thirstDecreaseMultiplier = 1f;

    [Header("Stamina Recovery")]
    public bool modifyStaminaRecovery;
    public float staminaRecoveryMultiplier = 1f;

    public override string GetEffectDescription()
    {
        var parts = new System.Collections.Generic.List<string>();
        parts.Add($"{duration}초 동안");
        if (modifyHunger) parts.Add($"배고픔 감소 x{hungerDecreaseMultiplier}");
        if (modifyThirst) parts.Add($"수분 감소 x{thirstDecreaseMultiplier}");
        if (modifyStaminaRecovery) parts.Add($"스태미나 회복 x{staminaRecoveryMultiplier}");
        return string.Join("  ", parts);
    }

    public override bool OnUse(PlayerStateMachine player)
    {
        if (player.timedEffectCoroutine != null)
            player.StopCoroutine(player.timedEffectCoroutine);
        player.timedEffectCoroutine = player.StartCoroutine(ApplyEffect(player));
        return true;
    }

    private IEnumerator ApplyEffect(PlayerStateMachine player)
    {
        var status = player.status;

        if (modifyHunger)
            status.hungerDecreasePerMinute = status.HungerDecayRate;
        if (modifyThirst)
            status.waterDecreasePerMinute = status.ThirstDecayRate;
        if (modifyStaminaRecovery)
            status.staminaRecoverey = status.StaminaRecoveryRate;

        yield return new WaitForSeconds(duration);

        if (modifyHunger)
            status.hungerDecreasePerMinute = status.baseHungerDecreasePerMinute;
        if (modifyThirst)
            status.waterDecreasePerMinute = status.baseWaterDecreasePerMinute;
        if (modifyStaminaRecovery)
            status.staminaRecoverey = status.baseStaminaRecovery;

        player.timedEffectCoroutine = null;
    }
}
