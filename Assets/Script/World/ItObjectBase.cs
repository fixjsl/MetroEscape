
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class ItObjectBase : MonoBehaviour, IInteractable
{
    public abstract bool isStuck { get; }
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
        TextObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (TextObject.activeSelf)
            TextObject.transform.rotation = Camera.main.transform.rotation;
    }


    public void OnInteract(Vector3 interacterPosition)
    {
        if (isInteracting || Time.time < lastInteractTime + interactCooldown) return;

        isInteracting = true;
        lastInteractTime = Time.time;
        OnInteractInternal(interacterPosition);
        hasInteracted = true;

   
            promptText.text = InteractMessage;

        if (currentPlayer != null && !gameObject.activeInHierarchy)
        {
            currentPlayer.ClearInteractable(this);
            currentPlayer = null;
        }
    }
    protected abstract void OnInteractInternal(Vector3 interacterPosition);

    public void RefreshPrompt()
    {
        if (TextObject != null && TextObject.activeInHierarchy)
            promptText.text = InteractMessage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        if(isInteracting) return;
        if (!other.TryGetComponent<PlayerStateMachine>(out var player)) return;
        currentPlayer = player;
        player.SetInteractable(this);
        promptText.text = InteractMessage;
        TextObject.SetActive(true);
        OnPlayerEntered(player);
    }

    protected virtual void OnPlayerEntered(PlayerStateMachine player) { }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<PlayerStateMachine>(out var player)) return;
        player.ClearInteractable(this);
        if (player == currentPlayer) currentPlayer = null;
        TextObject.SetActive(false);
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
            promptText.text = InteractMessage;
            TextObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (currentPlayer != null)
        {
            currentPlayer.ClearInteractable(this);
            currentPlayer = null;
        }
        TextObject.SetActive(false);
    }
}
