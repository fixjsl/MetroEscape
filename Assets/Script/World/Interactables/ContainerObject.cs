using System.Collections.Generic;
using UnityEngine;

public class ContainerObject : ItObjectBase
{
    [SerializeField] private LootTable lootTable;
    private bool open = false;
    private bool hasBeenOpened = false;
    private List<(ItemBase item, int count)> contents;

    [SerializeField] private Transform RotatePivot;
    [SerializeField] private string openMessage = "열기";
    [SerializeField] private string closeMessage = "닫기";
    [SerializeField] private bool disappearAfterOpen = false;
    [SerializeField] private bool excludeFromCycle = false;

    [Header("Sound")]
    [SerializeField] private AudioClip firstOpenSound;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    public override bool isStuck => false;
    public override string InteractMessage => open ? $"{closeMessage} [{InputBindings.Interact}]" : $"{openMessage} [{InputBindings.Interact}]";

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        isInteracting = false;

        if (open)
        {
            open = false;
            ContainerUI.Instance.Close();
            if (disappearAfterOpen && hasBeenOpened)
            {
                gameObject.SetActive(false);
                return;
            }
            RefreshPrompt();
            PlaySound(closeSound);
            return;
        }

        if (contents == null)
            contents = lootTable != null ? lootTable.Roll() : new List<(ItemBase, int)>();

        open = true;
        hasBeenOpened = true;
        ContainerUI.Instance.Open(contents);
        RefreshPrompt();
        PlayInteractSound(firstOpenSound, openSound);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (other.TryGetComponent<PlayerStateMachine>(out _))
        {
            open = false;
            ContainerUI.Instance.Close();
            if (disappearAfterOpen && hasBeenOpened)
                gameObject.SetActive(false);
        }
    }

    public void CycleReset()
    {
        if (excludeFromCycle) return;
        if (disappearAfterOpen && !gameObject.activeSelf)
            gameObject.SetActive(true);
        open = false;
        hasBeenOpened = false;
        contents = null;
    }
}
