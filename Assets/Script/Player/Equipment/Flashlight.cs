using UnityEngine;

[RequireComponent(typeof(Light))]
public class Flashlight : MonoBehaviour
{
    [Header("콘 설정")]
    [SerializeField] private float outerAngle   = 60f;
    [SerializeField] private float innerAngle   = 25f;
    [SerializeField] private float range        = 8f;
    [SerializeField] private float intensity    = 2f;
    [SerializeField] private float tiltDown     = 30f;
    [SerializeField] private float nearRange    = 1.8f;
    [SerializeField] private Color lightColor   = new Color(1f, 0.95f, 0.8f);
    [Header("바닥/천장 반사")]
    [SerializeField] private float floorOffsetY   = -1f;  // 플레이어 기준 바닥 오프셋
    [SerializeField] private float ceilingOffsetY =  3f;  // 플레이어 기준 천장 오프셋
    [SerializeField] private float surfaceBlur    =  0.03f; // 반사 하이라이트 두께
    [SerializeField] private Light spotLight;

    private float facingSign = 1f;

    void Start()
    {
        var player = PlayerStateMachine.Instance;
        if (player == null) return;
        player.EquipLight(spotLight);
        player.OnLightToggle += SetEnabled;
    }
    void Update()
    {
        UpdateShaderGlobals();       
    }

    void OnDestroy()
    {
        if (PlayerStateMachine.Instance != null)
            PlayerStateMachine.Instance.OnLightToggle -= SetEnabled;
    }

public void UpdateShaderGlobals()
{
    bool isOn = spotLight != null && spotLight.enabled;
    Shader.SetGlobalFloat("_FlashlightEnabled", isOn ? 1f : 0f);

    float moveInput = PlayerStateMachine.Instance.MoveInput;
    if (Mathf.Abs(moveInput) > 0.01f) facingSign = Mathf.Sign(moveInput);

    // 1. 플레이어의 3D 월드 위치를 '화면 2D Viewport 좌표 (0~1)'로 안전하게 변환
    Vector3 playerWorldPos = spotLight.transform.position;
    Vector3 playerScreenPos = Camera.main.WorldToViewportPoint(playerWorldPos);

    // 2. 캐릭터의 90도 회전에 방해받지 않는 무결한 2.5D 조준 끝점(3D 월드) 설정
    float cosAngle = Mathf.Cos(tiltDown * Mathf.Deg2Rad);
    float sinAngle = Mathf.Sin(tiltDown * Mathf.Deg2Rad);
    Vector3 lookDirWorld = new Vector3(facingSign * cosAngle, -sinAngle, 0f).normalized;
    Vector3 targetWorldPos = playerWorldPos + lookDirWorld;

    // 3. 조준 끝점 역시 '화면 2D Viewport 좌표 (0~1)'로 변환
    Vector3 targetScreenPos = Camera.main.WorldToViewportPoint(targetWorldPos);

    // 4. 화면 좌표계상에서의 무결한 2D 조준 방향 벡터 계산 (Z축 격리)
    Vector2 flashlightDir2D = (new Vector2(targetScreenPos.x, targetScreenPos.y) - new Vector2(playerScreenPos.x, playerScreenPos.y)).normalized;


    // [최종 데이터 가공 전송] 쉐이더에 철저히 화면용 2D 규격으로 배달합니다.
    Shader.SetGlobalVector("_FlashlightPos",        new Vector4(playerScreenPos.x, playerScreenPos.y, 0f, 0f));
    Shader.SetGlobalVector("_FlashlightDir",        new Vector4(flashlightDir2D.x, flashlightDir2D.y, 0f, 0f));
    Shader.SetGlobalFloat ("_FlashlightRange",      range * 0.05f);
    Shader.SetGlobalFloat ("_FlashlightOuterAngle", outerAngle);
    Shader.SetGlobalFloat ("_FlashlightInnerAngle", innerAngle);
    Shader.SetGlobalFloat ("_FlashlightIntensity",  intensity);
    Shader.SetGlobalColor ("_FlashlightColor",      lightColor);

    // 바닥/천장 월드 Y → 뷰포트 Y로 변환해서 전송
    Camera cam = Camera.main;
    float playerY   = spotLight.transform.position.y;
    float floorVP   = cam.WorldToViewportPoint(new Vector3(0, playerY + floorOffsetY,   0)).y;
    float ceilingVP = cam.WorldToViewportPoint(new Vector3(0, playerY + ceilingOffsetY, 0)).y;
    Shader.SetGlobalVector("_FloorCeilingY",   new Vector4(floorVP, ceilingVP, surfaceBlur, 0f));
}


    private void SetEnabled(bool on)
    {
        if (spotLight != null) spotLight.enabled = on;
        Shader.SetGlobalFloat("_FlashlightEnabled", on ? 1f : 0f);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Transform player = PlayerStateMachine.Instance != null
            ? PlayerStateMachine.Instance.transform
            : transform.parent;
        if (player == null) return;

        Vector3 dir      = Quaternion.AngleAxis(-tiltDown, player.right) * Vector3.down;
        Vector3 pos      = spotLight.transform.position ;
        float   cosOuter = Mathf.Cos(outerAngle * Mathf.Deg2Rad);

        // 광원 위치
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, 0.15f);

        // 중심 방향
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(pos, dir.normalized * range);

        // 콘 외곽선 (8개 방향)
        Gizmos.color = new Color(0f, 0.8f, 0.8f, 0.6f);
        Vector3 perp = Vector3.Cross(dir.normalized, player.up).normalized;
        if (perp.sqrMagnitude < 0.01f)
            perp = Vector3.Cross(dir.normalized, player.right).normalized;

        int segments = 12;
        for (int i = 0; i < segments; i++)
        {
            float   angle    = i * (360f / segments);
            Vector3 rotPerp  = Quaternion.AngleAxis(angle, dir.normalized) * perp;
            Vector3 edgeDir  = (dir.normalized * cosOuter + rotPerp * Mathf.Sqrt(1f - cosOuter * cosOuter)).normalized;
            Gizmos.DrawRay(pos, edgeDir * range);
        }

        // nearRange 구체
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(pos, nearRange);
    }
#endif
}
