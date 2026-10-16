

using UnityEngine;

public class Doorcs : ItObjectBase
{
    public override bool isStuck => false;
    public override string InteractMessage => isOpen ? $"닫기  [{InputBindings.Interact}]" : $"열기  [{InputBindings.Interact}]";

    protected override float interactCooldown => 0.5f;

    private Quaternion targetRot;
    private Quaternion closedRot;

    private bool isOpen = false;
    [SerializeField]
    private Collider triggerCollider;
    [SerializeField]
    private Transform offset;
    private Collider[] doorColliders;
    private Collider playerCollider;

    private void Awake()
    {
        closedRot = offset.rotation;
        targetRot = closedRot;
        doorColliders = offset.GetComponentsInChildren<Collider>(true);
    }

    private void SetIgnorePlayer(bool ignore)
    {
        if (playerCollider == null)
            playerCollider = PlayerStateMachine.Instance.GetComponent<Collider>();
        foreach (var col in doorColliders)
            if (!col.isTrigger) Physics.IgnoreCollision(col, playerCollider, ignore);
    }

    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private GameObject breakEffect; // 나중에 이펙트 연결용

    public void ForceClose()
    {
        if (!isOpen) return;
        targetRot = closedRot;
        isOpen = false;
        SetIgnorePlayer(true);
    }

    public void Break()
    {
        if (breakEffect != null)
            Instantiate(breakEffect, transform.position, transform.rotation);
        gameObject.SetActive(false);
    }

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        if (!isOpen)
        {
            Vector3 localPos = offset.transform.InverseTransformPoint(interacterPosition);
            float angle = localPos.x >= 0 ? 90f : -90f;
            targetRot = closedRot * Quaternion.Euler(0f, angle, 0f);
            isOpen = true;
            GameManager.Instance.PlaySFX(openSound);
        }
        else
        {
            targetRot = closedRot;
            isOpen = false;
            GameManager.Instance.PlaySFX(closeSound);
        }
        SetIgnorePlayer(true);
    }

    private void Update()
    {
        if (offset.transform.rotation == targetRot) return;

        offset.transform.rotation = Quaternion.RotateTowards(
            offset.transform.rotation, targetRot, 180f * Time.deltaTime);

        if (offset.transform.rotation == targetRot)
        {
            isInteracting = false;
            SetIgnorePlayer(false);
        }
    }

}

