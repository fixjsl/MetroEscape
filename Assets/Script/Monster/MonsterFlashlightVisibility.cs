using UnityEngine;

// 몬스터 루트 오브젝트에 붙이면 자식의 Renderer 를 자동으로 찾아서 사용.
// 손전등 콘 바깥 → 알파 0(투명), 콘 안쪽 → 알파 1(불투명) 으로 부드럽게 전환.
// 머티리얼 Surface Type 이 Transparent 여야 알파가 실제로 반영됨.
public class MonsterFlashlightVisibility : MonoBehaviour
{
    // 콘 진입/이탈 시 알파가 변하는 속도 (높을수록 순간적으로 등장/소멸)
    [SerializeField] private float fadeSpeed = 8f;

    private Renderer              _mr;   // 자식의 SkinnedMeshRenderer 등 모든 Renderer 호환
    private MaterialPropertyBlock _mpb;  // 머티리얼 인스턴스를 만들지 않고 알파만 덮어쓰는 블록
    private float                 _currentAlpha = 0f;
    private float                 _revealTimer   = 0f;   // 콘 이탈 후 남은 노출 시간
    private bool                  _wasInCone     = false; // 이전 프레임 콘 안 여부
    private const float           REVEAL_DURATION = 10f;  // 이탈 후 유지 시간(초)

    // 프로퍼티 이름을 매 프레임 문자열로 해싱하지 않도록 캐싱
    static readonly int ColorID = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        // 자식 계층 전체에서 Renderer 를 찾음 (MeshRenderer / SkinnedMeshRenderer 모두 가능)
        _mr  = GetComponentInChildren<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        bool inCone = IsInFlashlightCone();

        // 콘 안 → 타이머 초기화
        // 콘에서 막 이탈한 순간(이전 프레임엔 안쪽) → 10초 타이머 시작
        // 타이머가 남아있는 동안은 콘 밖이어도 계속 보임
        if (inCone)
            _revealTimer = 0f;
        else if (_wasInCone)
            _revealTimer = REVEAL_DURATION;
        else
            _revealTimer = Mathf.Max(0f, _revealTimer - Time.deltaTime);

        _wasInCone = inCone;

        float target  = (inCone || _revealTimer > 0f) ? 1f : 0f;
        _currentAlpha = Mathf.MoveTowards(_currentAlpha, target, fadeSpeed * Time.deltaTime);

        // MaterialPropertyBlock 으로 알파 적용 — 머티리얼 공유 인스턴스를 건드리지 않음
        _mr.GetPropertyBlock(_mpb);
        _mpb.SetColor(ColorID, new Color(1f, 1f, 1f, _currentAlpha));
        _mr.SetPropertyBlock(_mpb);
    }

    // Flashlight.cs 가 SetGlobal 로 보낸 값으로 CPU 에서 콘 판정
    // 쉐이더와 동일한 수식이지만 GPU 연산 없이 오브젝트 단위로만 계산하므로 가볍다
    bool IsInFlashlightCone()
    {
        // 손전등이 꺼져 있으면 항상 숨김
        if (Shader.GetGlobalFloat("_FlashlightEnabled") < 0.5f) return false;

        Camera cam = Camera.main;
        if (cam == null) return false;

        // 몬스터 월드 위치 → 뷰포트 좌표 (0~1)
        Vector3 vp3 = cam.WorldToViewportPoint(transform.position);
        if (vp3.z < 0f) return false;  // 카메라 뒤쪽이면 제외
        Vector2 vp = new Vector2(vp3.x, vp3.y);

        // 손전등 글로벌 값 읽기
        Vector2 flashPos = Shader.GetGlobalVector("_FlashlightPos");
        Vector2 flashDir = Shader.GetGlobalVector("_FlashlightDir");
        float   range    = Shader.GetGlobalFloat("_FlashlightRange");
        float   outer    = Shader.GetGlobalFloat("_FlashlightOuterAngle");

        // 종횡비 보정 후 손전등 좌표계로 분해
        float   aspect   = (float)Screen.width / Screen.height;
        Vector2 uvA      = new(vp.x * aspect, vp.y);
        Vector2 originA  = new(flashPos.x * aspect, flashPos.y);
        Vector2 fwd      = flashDir.normalized;
        Vector2 delta    = uvA - originA;
        float   along    = Vector2.Dot(delta, fwd);

        // 손전등 뒤쪽이거나 사거리 초과면 즉시 false
        if (along < 0f) return false;
        if (delta.magnitude > range + 0.04f) return false;

        // 각도 판정: pixelTan 이 outerTan 이내면 콘 안쪽
        // 1.45 배 여유를 줘서 경계 근처에서 너무 일찍 사라지지 않게 함
        float perp     = Mathf.Abs(Vector2.Dot(delta, new Vector2(-fwd.y, fwd.x)));
        float pixelTan = perp / Mathf.Max(along, 1e-5f);
        float outerTan = Mathf.Tan(outer * 0.5f * Mathf.Deg2Rad);

        return pixelTan <= outerTan * 1.45f;
    }
}
