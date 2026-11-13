using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // 따라다닐 대상 (플레이어)

    [Header("Position Settings")]
    public Vector3 offset  = new  Vector3(0,14,-65);// 카메라 위치 오프셋
    public float smoothSpeed = 0.125f; // 따라가는 부드러움 (0~1, 높을수록 빠름)

    [Header("Rotation Settings")]
    public bool lookAtTarget = true; // 대상 바라보기 여부
    public float rotationSpeed = 5f; // 회전 속도

    [Header("Look Ahead")]
    [Range(0f, 1f)]
    public float screenThird = 0.333f;    // 0.333 = 1/3 지점, 0 = 중앙
    public float lookAheadSpeed = 3f;     // 오프셋 전환 속도

    private float currentLookAhead = 0f;
    private Camera cam;

    void Awake() => cam = GetComponent<Camera>();
    void Start(){
        if (target == null) return;
        currentLookAhead = Mathf.Sign(target.forward.x) * CalcLookAhead();
        transform.position = new Vector3(
        target.position.x + offset.x + currentLookAhead,
        target.position.y + offset.y,
        transform.position.z);
    }
    float CalcLookAhead()
    {
        float dist = Mathf.Abs(transform.position.z - target.position.z);
        float halfWidth = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * dist * cam.aspect;
        return halfWidth * screenThird * 2f;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float facingSign = Mathf.Sign(target.forward.x);
        float targetLookAhead = facingSign * CalcLookAhead();
        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);

        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x + currentLookAhead,
            target.position.y + offset.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed*Time.deltaTime);
    }

    // 런타임에 대상 설정
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}

