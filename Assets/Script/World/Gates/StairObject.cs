using Unity.VisualScripting;
using UnityEngine;

public class StairObject : MonoBehaviour
{
    [SerializeField] private Transform stairOffset;
    [SerializeField] private Transform stairBottom;
    [SerializeField] private Transform stairTop;
    [SerializeField] private float dismountYOffset = 0f;
    [SerializeField] private float stairYSpeed = 8f;
private PlayerStateMachine mountedPlayer;
    private float originalZ;
    private StairTrigger mountedFrom;
    public bool IsOnStair { get; private set; }

    public void OnPlayerEnterTrigger(StairTrigger trigger, PlayerStateMachine player)
    {
        if (!IsOnStair) return;

        if (trigger != mountedFrom)
        {
            // 반대편 끝에 도착 → 하차
            var prev = mountedFrom;
            mountedFrom = null;
            Dismount(player);
            if (prev != null) prev.RefreshPrompt();
        }
        else
        {
            // 탑승 위치로 되돌아옴 → 하차
            mountedFrom = null;
            Dismount(player);
            trigger.RefreshPrompt();
        }
    }

    private void Update()
    {
    if (!IsOnStair || mountedPlayer == null || stairBottom == null || stairTop == null) return;

    float xMin = Mathf.Min(stairBottom.position.x, stairTop.position.x);
    float xMax = Mathf.Max(stairBottom.position.x, stairTop.position.x);
    float t = Mathf.Clamp01(Mathf.InverseLerp(xMin, xMax, mountedPlayer.Rb.position.x));

    float yTarget = stairBottom.position.x < stairTop.position.x
        ? Mathf.Lerp(stairBottom.position.y, stairTop.position.y, t)
        : Mathf.Lerp(stairTop.position.y, stairBottom.position.y, t);

    var pos = mountedPlayer.Rb.position;
    float stairLength = Mathf.Max(0.01f, Mathf.Abs(stairTop.position.x - stairBottom.position.x));
    float slope = Mathf.Abs(stairTop.position.y - stairBottom.position.y) / stairLength;
    float ySpeed = Mathf.Max(stairYSpeed, Mathf.Abs(mountedPlayer.Rb.linearVelocity.x) * slope);
    pos.y = Mathf.MoveTowards(pos.y, yTarget, ySpeed * Time.deltaTime);
    mountedPlayer.Rb.position = pos;
    }
    public void TryInteract(StairTrigger trigger, PlayerStateMachine player)
    {
        if (!IsOnStair)
        {
            mountedFrom = trigger;
            Mount(player);
        }
        else
        {
            var prev = mountedFrom;
            mountedFrom = null;
            Dismount(player);
            if (prev != null) prev.RefreshPrompt();
        }
    }

    private void Mount(PlayerStateMachine player)
    {
        originalZ = player.Rb.position.z;
        IsOnStair = true;
        mountedPlayer = player;
        var pos = player.Rb.position;
        
        pos.z = stairOffset.position.z;
        player.Rb.position = pos;
    }

    private void Dismount(PlayerStateMachine player)
    {
        IsOnStair = false;
        mountedPlayer = null;
        player.Rb.linearVelocity = Vector3.zero;

        var pos = player.Rb.position;
        pos.z = originalZ;

        if (stairBottom != null && stairTop != null)
        {
            float distToBottom = Mathf.Abs(pos.x - stairBottom.position.x);
            float distToTop = Mathf.Abs(pos.x - stairTop.position.x);
            pos.y = (distToBottom < distToTop ? stairBottom.position.y : stairTop.position.y) + dismountYOffset;
        }

        player.Rb.position = pos;
    }
}
