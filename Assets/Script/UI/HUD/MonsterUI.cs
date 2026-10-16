using UnityEngine;
using UnityEngine.UI;

public class MonsterUI : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private Image hpFill;
    [SerializeField] private Image ghostFill;   // 데미지 표시용 (노랑/주황)
    [SerializeField] private float ghostDelay = 0.5f;   // 줄어들기 시작까지 대기
    [SerializeField] private float ghostSpeed = 2f;     // 줄어드는 속도

    [Header("Detection")]
    [SerializeField] private Image detectionFill;
    [SerializeField] private Image detect;

    private MonsterStateMachine monster;
    private Transform hpAnchor;
    private Transform detectionAnchor;
    private System.Action<float> onDetectionChanged;

    private float ghostTimer;
    private bool ghostWaiting;

    private CanvasGroup _cg;          // UI 전체 알파 제어용
    private float       _revealTimer  = 0f;
    private bool        _wasInCone    = false;
    private const float REVEAL_DURATION = 10f;

    public void Init(MonsterStateMachine target, Transform hpPoint, Transform detectionPoint)
    {
        monster = target;
        hpAnchor = hpPoint;
        detectionAnchor = detectionPoint;
        detect.gameObject.SetActive(false);

        // UICamera를 Canvas에 자동 연결
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            Camera uiCam = GameObject.Find("UICamera")?.GetComponent<Camera>();
            if (uiCam != null) canvas.worldCamera = uiCam;
        }

        // CanvasGroup 이 없으면 자동으로 추가
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
        _cg.alpha = 0f;

        monster.status.ChangeHP += OnHPChanged;
        monster.status.OnDie += OnMonsterDie;
        onDetectionChanged = (gauge) =>
        {
            if (detectionFill != null)
                detectionFill.fillAmount = gauge / 100f;
            if (gauge >= 100f && detect != null)
                detect.gameObject.SetActive(true);
        };
        monster.status.ChangeDetectionGauge += onDetectionChanged;
        OnHPChanged(monster.status.Hp);
        detectionFill.fillAmount = 0f;
    }

    // MonsterFlashlightVisibility 와 동일한 콘 판정 — 몬스터 위치 기준
    bool IsInFlashlightCone()
    {
        if (Shader.GetGlobalFloat("_FlashlightEnabled") < 0.5f) return false;

        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 vp3 = cam.WorldToViewportPoint(monster.transform.position);
        if (vp3.z < 0f) return false;

        Vector2 vp       = new(vp3.x, vp3.y);
        Vector2 flashPos = Shader.GetGlobalVector("_FlashlightPos");
        Vector2 flashDir = Shader.GetGlobalVector("_FlashlightDir");
        float   range    = Shader.GetGlobalFloat("_FlashlightRange");
        float   outer    = Shader.GetGlobalFloat("_FlashlightOuterAngle");

        float   aspect   = (float)Screen.width / Screen.height;
        Vector2 uvA      = new(vp.x * aspect, vp.y);
        Vector2 originA  = new(flashPos.x * aspect, flashPos.y);
        Vector2 fwd      = flashDir.normalized;
        Vector2 delta    = uvA - originA;
        float   along    = Vector2.Dot(delta, fwd);

        if (along < 0f) return false;
        if (delta.magnitude > range + 0.04f) return false;

        float perp     = Mathf.Abs(Vector2.Dot(delta, new Vector2(-fwd.y, fwd.x)));
        float pixelTan = perp / Mathf.Max(along, 1e-5f);
        float outerTan = Mathf.Tan(outer * 0.5f * Mathf.Deg2Rad);

        return pixelTan <= outerTan * 1.45f;
    }

    void Update()
    {
        if (monster == null) return;

        // 몬스터 본체와 동일한 기준으로 UI 알파 제어
        if (_cg != null)
        {
            bool inCone = IsInFlashlightCone();

            if (inCone)
                _revealTimer = 0f;
            else if (_wasInCone)
                _revealTimer = REVEAL_DURATION;
            else
                _revealTimer = Mathf.Max(0f, _revealTimer - Time.deltaTime);

            _wasInCone = inCone;

            float target = (inCone || _revealTimer > 0f) ? 1f : 0f;
            _cg.alpha = Mathf.MoveTowards(_cg.alpha, target, 8f * Time.deltaTime);
        }

        // 고스트 HP 처리
        if (ghostFill != null && ghostWaiting)
        {
            ghostTimer -= Time.deltaTime;
            if (ghostTimer <= 0f)
            {
                ghostWaiting = false;
                ghostFill.fillAmount = Mathf.MoveTowards(ghostFill.fillAmount, hpFill.fillAmount, ghostSpeed * Time.deltaTime);
            }
        }
        else if (ghostFill != null && ghostFill.fillAmount > hpFill.fillAmount)
        {
            ghostFill.fillAmount = Mathf.MoveTowards(ghostFill.fillAmount, hpFill.fillAmount, ghostSpeed * Time.deltaTime);
        }

        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;

            if (hpAnchor != null)
            {
                hpFill.transform.parent.position = hpAnchor.position;
            }
            if (detectionAnchor != null)
            {
                detectionFill.transform.parent.position = detectionAnchor.position;
            }
        }
    }

    void OnHPChanged(float hp)
    {
        if (hpFill == null) return;

        float newFill = hp / monster.status.Maxhp;

        // 데미지일 때만 고스트 활성화
        if (newFill < hpFill.fillAmount)
        {
            ghostWaiting = true;
            ghostTimer = ghostDelay;
        }

        hpFill.fillAmount = newFill;
    }

    void OnMonsterDie()
    {
        gameObject.SetActive(false);
    }

    public void ResetForRespawn()
    {
        gameObject.SetActive(true);
        _cg.alpha = 0f;
        _revealTimer = 0f;
        _wasInCone = false;
        detect.gameObject.SetActive(false);
        detectionFill.fillAmount = 0f;
        OnHPChanged(monster.status.Maxhp);
    }

    void OnDestroy()
    {
        if (monster != null)
        {
            monster.status.ChangeHP -= OnHPChanged;
            monster.status.OnDie -= OnMonsterDie;
            monster.status.ChangeDetectionGauge -= onDetectionChanged;
        }
    }
}
