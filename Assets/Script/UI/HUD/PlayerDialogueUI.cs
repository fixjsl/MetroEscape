using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerDialogueUI : SingletonBehaviour<PlayerDialogueUI>
{

    [SerializeField] private GameObject bubble;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float cooldown = 30f;
    [SerializeField] private float charDelay = 0.05f;
    [SerializeField] private Vector2 bubblePadding = new(24f, 16f);

    private RectTransform bubbleRect;
    private Coroutine hideCoroutine;
    private Coroutine typewriterCoroutine;
    private string lastText;
    private float lastShowTime = -999f;

    private float lastHungryTime = -999f;
    private float lastWaterTime  = -999f;
    private float lastHPTime     = -999f;

    private Action onInventoryFull;
    private Action onSlotsFull;
    private Action onStaminaEmpty;
    private Action onParrySuccess;
    private Action<int>   onHungryChanged;
    private Action<int>   onWaterChanged;
    private Action<float> onHPChanged;

    protected override void Awake()
    {
        base.Awake();
        bubbleRect = bubble.GetComponent<RectTransform>();
        bubble.SetActive(false);
    }

    void Start()
    {
        var player = PlayerStateMachine.Instance;

        onInventoryFull  = () => Show("가방이 꽉 찼어");
        onSlotsFull      = () => Show("더 이상 넣을 수 없어");
        onHungryChanged  = val => OnHungryChanged(val);
        onWaterChanged   = val => OnWaterChanged(val);
        onHPChanged      = val => OnHPChanged(val);

        player.inventory.OnInventoryFull += onInventoryFull;
        player.inventory.OnSlotsFull     += onSlotsFull;
        player.status.ChangeHungry       += onHungryChanged;
        player.status.ChangeWater        += onWaterChanged;
        player.status.ChangeHP           += onHPChanged;
    }

    void LateUpdate()
    {
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }

    void OnHungryChanged(int val)
    {
        if (val <= 20 && Time.time - lastHungryTime > cooldown)
        {
            Show("배가 고프다");
            lastHungryTime = Time.time;
        }
    }

    void OnWaterChanged(int val)
    {
        if (val <= 20 && Time.time - lastWaterTime > cooldown)
        {
            Show("목이 말라온다");
            lastWaterTime = Time.time;
        }
    }

    void OnHPChanged(float val)
    {
        float ratio = val / PlayerStateMachine.Instance.status.Maxhp;
        if (ratio <= 0.25f && Time.time - lastHPTime > cooldown)
        {
            Show("방호복이 망가져간다");
            lastHPTime = Time.time;
        }
    }

    // 기존 즉시 표시 (배고픔 등 상태 대사용)
    public void Show(string text)
    {
        if (text == lastText && Time.time - lastShowTime < 5f) return;

        lastText     = text;
        lastShowTime = Time.time;

        StopAll();
        dialogueText.text = text;
        UpdateBubbleSize(text);
        bubble.SetActive(true);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    // 한 줄 타이핑 효과
    public void ShowTypewriter(string text)
    {
        StopAll();
        bubble.SetActive(true);
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(new[] { text }, displayDuration));
    }

    // 여러 줄 순서대로 타이핑 효과 (대사 배열, 줄 사이 대기 시간)
    public void ShowSequence(string[] lines, float linePause = 2f)
    {
        StopAll();
        bubble.SetActive(true);
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(lines, linePause));
    }

    private IEnumerator TypewriterRoutine(string[] lines, float linePause)
    {
        foreach (string line in lines)
        {
            dialogueText.text = "";

            foreach (char c in line)
            {
                dialogueText.text += c;
                UpdateBubbleSize(dialogueText.text);
                yield return new WaitForSeconds(charDelay);
            }

            yield return new WaitForSeconds(linePause);
        }

        typewriterCoroutine = null;
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private void UpdateBubbleSize(string text)
    {
        if (bubbleRect == null) return;
        Vector2 size = dialogueText.GetPreferredValues(text);
        bubbleRect.sizeDelta = size + bubblePadding * 2f;
    }

    private void StopAll()
    {
        if (hideCoroutine != null) { StopCoroutine(hideCoroutine); hideCoroutine = null; }
        if (typewriterCoroutine != null) { StopCoroutine(typewriterCoroutine); typewriterCoroutine = null; }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        bubble.SetActive(false);
        hideCoroutine = null;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PlayerStateMachine.Instance == null) return;
        var player = PlayerStateMachine.Instance;

        player.inventory.OnInventoryFull -= onInventoryFull;
        player.inventory.OnSlotsFull     -= onSlotsFull;
        player.status.StaminaEmpty       -= onStaminaEmpty;
        player.ParrySuccess              -= onParrySuccess;
        player.status.ChangeHungry       -= onHungryChanged;
        player.status.ChangeWater        -= onWaterChanged;
        player.status.ChangeHP           -= onHPChanged;
    }
}
