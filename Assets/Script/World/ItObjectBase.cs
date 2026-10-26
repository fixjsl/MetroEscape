
using TMPro;
using UnityEngine;

public abstract class ItObjectBase : MonoBehaviour, IInteractable
{
    public abstract string InteractMessage { get; }
    [SerializeField] private GameObject TextObject;
    [SerializeField] private TextMeshPro promptText;

    protected bool isInteracting = false;
    protected bool hasInteracted = false;
    private PlayerStateMachine currentPlayer;
    private float lastInteractTime = -Mathf.Infinity;

    protected virtual float interactCooldown => 0f;

    protected virtual void Start()
    {
        SetPromptVisible(false);
    }

    void LateUpdate()
    {
        if (TextObject != null && TextObject.activeSelf && Camera.main != null)
            TextObject.transform.rotation = Camera.main.transform.rotation;
    }

    protected void BeginBusy()
    {
        isInteracting = true;
    }
    protected void EndBusy()
    {
        isInteracting = false;
        RefreshPrompt();
    }
    protected string Prompt(string action)
    {
        return $"{action} [{InputBindings.Interact}]";
    }
 
    public void OnInteract(Vector3 interacterPosition)
    {
        if (isInteracting || Time.time < lastInteractTime + interactCooldown) return;

        lastInteractTime = Time.time;
        OnInteractInternal(interacterPosition);
        hasInteracted = true;


        RefreshPrompt();

        if (currentPlayer != null && !gameObject.activeInHierarchy)
        {
            currentPlayer.ClearInteractable(this);
            currentPlayer = null;
        }
    }
    protected abstract void OnInteractInternal(Vector3 interacterPosition);

    public void RefreshPrompt()
    {
        if (currentPlayer != null) ShowPrompt();   // 플레이어가 범위 안에 있을 때만 (빈 문구면 숨김)
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        if(isInteracting) return;
        if (!other.TryGetComponent<PlayerStateMachine>(out var player)) return;
        currentPlayer = player;
        player.SetInteractable(this);
        ShowPrompt();
        OnPlayerEntered(player);
    }

    protected virtual void OnPlayerEntered(PlayerStateMachine player) { }
    protected virtual void OnPlayerExited(PlayerStateMachine player) { }
    protected static bool PlayerHas(ItemBase item)
    {
        return item != null && PlayerStateMachine.Instance != null && PlayerStateMachine.Instance.inventory.slots.ContainsKey(item.itemcode);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<PlayerStateMachine>(out var player)) return;
        player.ClearInteractable(this);
        if (player == currentPlayer) currentPlayer = null;
        SetPromptVisible(false);
        OnPlayerExited(player);   // 자식은 이 훅만 override
    }

    protected void PlaySound(AudioClip clip)
    {
        if (clip != null)
            GameManager.Instance.PlaySFX(clip);
    }

    protected void PlayInteractSound(AudioClip firstClip, AudioClip repeatClip)
    {
        PlaySound(hasInteracted ? repeatClip : firstClip);
    }
    protected void TogglePanel(UIPanel panel)
    {
        if (panel.IsOpen) panel.Close(); else panel.Open();
    }
    private void SetPromptVisible(bool on)
    {
        if (TextObject != null) TextObject.SetActive(on);   // 인스펙터에서 빠뜨려도 NRE 안 나게
    }
    private void ShowPrompt()
    {
        string msg = InteractMessage;
        if (string.IsNullOrEmpty(msg)) { SetPromptVisible(false); }
        else { if (promptText != null) promptText.text = msg; SetPromptVisible(true); }
    }
    private void OnEnable()
    {
        var col = GetComponent<Collider>();
        if (col == null) return;
        var player = PlayerStateMachine.Instance;
        if (player == null) return;
        if (col.bounds.Contains(player.transform.position))
        {
            currentPlayer = player;
            player.SetInteractable(this);
            ShowPrompt();
        }
    }

    private void OnDisable()
    {
        if (currentPlayer != null)
        {
            currentPlayer.ClearInteractable(this);
            currentPlayer = null;
        }
        SetPromptVisible(false);
    }
}
