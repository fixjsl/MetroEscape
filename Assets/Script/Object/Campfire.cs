using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : ItObjectBase
{
    [SerializeField] private ParticleSystem fireParticle;
    [SerializeField] private Light fireLight;
    [SerializeField] private ScreenPointLight screenLight;
    [SerializeField] private AudioSource fireAudioSource;
    [SerializeField] private Sprite fuelIcon;
    [SerializeField] private int requiredFuelCount = 3;
    [SerializeField] private ItemBase lighterItem;
    [SerializeField] private ItemBase rawMeat;
    [SerializeField] private ItemBase cookedMeat;

    public bool IsBurning { get; private set; }
    public Sprite FuelIcon => fuelIcon;
    public int RequiredFuelCount => requiredFuelCount;
    public ItemBase LighterItem => lighterItem;
    public ItemBase RawMeat => rawMeat;

    protected override void Start()
    {
        base.Start();
        if (screenLight != null) screenLight.enabled = false;
    }

    public override bool isStuck => false;
    public override string InteractMessage =>
        IsBurning ? $"요리하기 {InputBindings.Interact}" : $"불피우기 {InputBindings.Interact}";

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        if (CampfireUI.Instance.IsOpen)
            CampfireUI.Instance.Close();
        else
            CampfireUI.Instance.Open(this);
        isInteracting = false;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (other.TryGetComponent<PlayerStateMachine>(out _))
            CampfireUI.Instance.Close();
    }

    public static int CountFuel(Inventory inv)
    {
        int total = 0;
        foreach (var (code, count) in inv.slots)
        {
            if (!ItemManager.itemDB.TryGetValue(code, out var item)) continue;
            if (item.canBurnAsFuel) total += count;
        }
        return total;
    }

    // 연료 소비 순서: 인벤토리 획득 순서(맨 위)부터 차례로 소비됨
    private bool ConsumeFuel(int amount)
    {
        var inv = PlayerStateMachine.Instance.inventory;
        int remaining = amount;
        var toRemove = new List<(ItemBase item, int count)>();
        foreach (var (code, count) in inv.slots)
        {
            if (remaining <= 0) break;
            if (!ItemManager.itemDB.TryGetValue(code, out var item)) continue;
            if (!item.canBurnAsFuel) continue;
            int take = Mathf.Min(count, remaining);
            toRemove.Add((item, take));
            remaining -= take;
        }
        if (remaining > 0) return false;
        foreach (var (item, count) in toRemove)
            inv.RemoveItem(item, count);
        return true;
    }

    public void LightFire()
    {
        var inv = PlayerStateMachine.Instance.inventory;
        if (lighterItem != null && !inv.RemoveItem(lighterItem, 1)) return;
        if (!ConsumeFuel(requiredFuelCount)) return;

        IsBurning = true;
        if (fireParticle != null) fireParticle.Play();
        if (fireLight != null) fireLight.enabled = true;
        if (screenLight != null) screenLight.enabled = true;
        if (fireAudioSource != null) fireAudioSource.Play();
        RefreshPrompt();
        StartCoroutine(BurnRoutine());
        CampfireUI.Instance.Refresh(this);
    }

    public void CookMeat()
    {
        var inv = PlayerStateMachine.Instance.inventory;
        if (!inv.RemoveItem(rawMeat, 1)) return;
        if (!inv.AddItem(cookedMeat)) inv.AddItem(rawMeat, 1, false);
        CampfireUI.Instance.Refresh(this);
    }

    private IEnumerator BurnRoutine()
    {
        yield return new WaitForSecondsRealtime(60f);
        IsBurning = false;
        if (fireParticle != null) fireParticle.Stop();
        if (fireLight != null) fireLight.enabled = false;
        if (screenLight != null) screenLight.enabled = false;
        if (fireAudioSource != null) fireAudioSource.Stop();
        RefreshPrompt();
        if (CampfireUI.Instance.IsOpen)
            CampfireUI.Instance.Refresh(this);
    }
}
