using System.Collections;
using DG.Tweening;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    private TutorialTriggerType? pendingTrigger = null;
    private MonsterStateMachine tutorialWolf;

    [SerializeField] private GameObject targetPlayer;
    [SerializeField] private GameObject moveKeyUI;
    [SerializeField] private GameObject sprintKeyUI;
    [SerializeField] private GameObject interactKeyUI;
    [SerializeField] private GameObject attackKeyUI;
    [SerializeField] private GameObject dodgeKeyUI;
    [SerializeField] private GameObject parryKeyUI;
    [SerializeField] private GameObject GetItemUI;
    [SerializeField] private GameObject crunchKeyUI;
    [SerializeField] private float keyUIDisplayTime = 3f;

    [SerializeField] private Doorcs weaponRoomDoor;
    [SerializeField] private Transform playerStopPoint;
    [SerializeField] private GameObject weaponCardContainer;
    [SerializeField] private GameObject wolfPrefab;
    [SerializeField] private Transform wolfSpawnPoint;
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform monsterSpawnPoint;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(TutorialSequence());
    }

    public void OnTrigger(TutorialTriggerType type)
    {
        pendingTrigger = type;
    }

    // 특정 트리거가 들어올 때까지 대기
    IEnumerator WaitForTrigger(TutorialTriggerType type)
    {
        while (pendingTrigger != type)
            yield return new WaitForSecondsRealtime(0.05f);
        pendingTrigger = null;
    }

    // ===================== 메인 시퀀스 =====================

    IEnumerator TutorialSequence()
    {
        GameManager.Instance.TutorialStart();

        // 1단계: 이동키 튜토리얼
        yield return WaitForTrigger(TutorialTriggerType.ShowMoveKeyNdialogue);
        if (moveKeyUI != null) moveKeyUI.SetActive(true);

        // 2단계: 늑대 등장
        yield return WaitForTrigger(TutorialTriggerType.WolfFearNdialogue);
        if (moveKeyUI != null) moveKeyUI.SetActive(false);
        if (wolfPrefab != null && wolfSpawnPoint != null)
        {
            var wolfObj = Instantiate(wolfPrefab, wolfSpawnPoint.position, wolfPrefab.transform.rotation);
            if (wolfObj.TryGetComponent<MonsterStateMachine>(out var wolf))
            {
                tutorialWolf = wolf;
                wolf.SetTarget(PlayerStateMachine.Instance);
                wolf.ChangeState<MonsterStates.Chase>();
            }
        }

        // 3단계: 달리기 키 튜토리얼
        yield return WaitForTrigger(TutorialTriggerType.ShowSprintKey);
        if (sprintKeyUI != null) sprintKeyUI.SetActive(true);

        // 4단계: 상호작용 키 튜토리얼
        yield return WaitForTrigger(TutorialTriggerType.DialogueNShowInteractKey);
        if (sprintKeyUI != null) sprintKeyUI.SetActive(false);
        if (interactKeyUI != null) interactKeyUI.SetActive(true);

        // 5단계: 무기 선택방 진입
        yield return WaitForTrigger(TutorialTriggerType.WeaponRoom);
        if (interactKeyUI != null) interactKeyUI.SetActive(false);

        // 문 자동 닫기 + 플레이어 천천히 멈추기
        if (weaponRoomDoor != null) weaponRoomDoor.ForceClose();
        var player = PlayerStateMachine.Instance;
        var rb = player.Rb;

        player.SetMovementLocked(true);
        rb.linearVelocity = Vector3.zero;

        if (playerStopPoint != null)
        {
            Vector3 target = new Vector3(playerStopPoint.position.x, rb.position.y, rb.position.z);
            yield return rb.DOMove(target, 1.5f).SetEase(Ease.InOutSine).WaitForCompletion();
        }
        else
            yield return new WaitForSecondsRealtime(1.5f);

        player.ChangeState<Idle>();
        yield return new WaitForSecondsRealtime(0.3f);

        // 카드 표시 + 시간 정지
        if (weaponCardContainer != null) weaponCardContainer.SetActive(true);
        Time.timeScale = 0f;

        // 카드 선택 대기 (SelectWeapon에서 timeScale 1로 복구 + OnTrigger 호출)
        yield return WaitForTrigger(TutorialTriggerType.WeaponSelected);
        PlayerStateMachine.Instance.SetMovementLocked(false);

        // 무기 선택 후 장착하면서 전투시작

        // 6단계: 전투 시작
        yield return WaitForTrigger(TutorialTriggerType.CombatStart);
        if (weaponRoomDoor != null) weaponRoomDoor.Break();
        if (tutorialWolf != null)
        {
            tutorialWolf.SetTarget(PlayerStateMachine.Instance);
            tutorialWolf.ChangeState<MonsterStates.Chase>();
        }

        // 7~9단계: 공격/회피/패링 키 순서대로 표시
        yield return ShowKeyUI(attackKeyUI);
        yield return ShowKeyUI(dodgeKeyUI);
        yield return ShowKeyUI(parryKeyUI);

        //10단계 : 아이템 획득, 사용 및 퀵슬롯 튜토리얼
        yield return WaitForTrigger(TutorialTriggerType.GetItem);
        if (GetItemUI != null) GetItemUI.SetActive(true);

        // 11단계: 웅크리기 튜토리얼
        yield return WaitForTrigger(TutorialTriggerType.DialogeNShowCrunchKey);
        yield return ShowKeyUI(crunchKeyUI);

        //12단계 : 곰 출몰 후 도망
        yield return WaitForTrigger(TutorialTriggerType.ApperBear);
        //13단계 : 도망치는 곳의 벽이 무너짐 ( 계단을 따라 지하로 도망침)

        //14단계 : 기지 진입 후 진입한 곳의 벽이 무너짐(안전지대 진입 및 프롤로그, 튜토리얼 종료, 이후 스킵 가능 생성)
        yield return WaitForTrigger(TutorialTriggerType.TutorialEnd);
        // 튜토리얼 종료
        EndTutorial();
    }

    IEnumerator ShowKeyUI(GameObject ui)
    {
        if (ui == null) yield break;
        ui.SetActive(true);
        yield return new WaitForSeconds(keyUIDisplayTime);
        ui.SetActive(false);
    }

    void EndTutorial()
    {
        // TODO: 튜토리얼 종료 처리
        Debug.Log("튜토리얼 완료");
    }
}
