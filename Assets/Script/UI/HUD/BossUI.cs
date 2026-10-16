using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 캔버스에 배치하는 보스 전용 UI
// 전투 시작 시 Init(), 전투 종료 시 Hide() 호출
public class BossUI : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private Image hpFill;
    [SerializeField] private Image ghostFill;
    [SerializeField] private float ghostDelay = 0.5f;
    [SerializeField] private float ghostSpeed = 2f;

    [Header("Info")]
    [SerializeField] private TMP_Text bossNameText;

    [Header("Animation")]
    [SerializeField] private float fadeSpeed = 3f;

    private CanvasGroup canvasGroup;
    private MonsterStatus linkedStatus;
    private float ghostTimer;
    private bool ghostWaiting;
    private bool visible;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    // BossStateMachine.OnPlayerEnterZone 에서 호출
    public void Init(MonsterStatus status)
    {
        // 이전 구독 해제
        if (linkedStatus != null)
            linkedStatus.ChangeHP -= OnHPChanged;

        linkedStatus = status;
        linkedStatus.ChangeHP += OnHPChanged;

        if (bossNameText != null)
            bossNameText.text = status.Name;

        hpFill.fillAmount = 1f;
        if (ghostFill != null) ghostFill.fillAmount = 1f;

        visible = true;
    }

    // BossStateMachine.OnPlayerEscapedZone 에서 호출
    public void Hide()
    {
        visible = false;
    }

    private void Update()
    {
        float targetAlpha = visible ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        if (ghostFill == null) return;

        if (ghostWaiting)
        {
            ghostTimer -= Time.deltaTime;
            if (ghostTimer <= 0f)
                ghostWaiting = false;
        }
        else if (ghostFill.fillAmount > hpFill.fillAmount)
        {
            ghostFill.fillAmount = Mathf.MoveTowards(ghostFill.fillAmount, hpFill.fillAmount, ghostSpeed * Time.deltaTime);
        }
    }

    private void OnHPChanged(float hp)
    {
        if (linkedStatus == null) return;
        float newFill = hp / linkedStatus.Maxhp;

        if (newFill < hpFill.fillAmount)
        {
            ghostWaiting = true;
            ghostTimer = ghostDelay;
        }

        hpFill.fillAmount = newFill;
    }

    private void OnDestroy()
    {
        if (linkedStatus != null)
            linkedStatus.ChangeHP -= OnHPChanged;
    }
}
