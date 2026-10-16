using UnityEngine;
public enum TutorialTriggerType
{
    ShowMoveKeyNdialogue,//게임 시작시 첫 대사 및 이동키 튜토리얼
    WolfFearNdialogue,//늑대가 플레이어 발견하고 쫓아옴, 그리고 대사
    ShowSprintKey,//달리기 키 튜토리얼
    DialogueNShowInteractKey,//대사 및 상호작용키 튜토리얼
    WeaponRoom,// 무기 선택카드 및 문 닫힘
    WeaponSelected,// 무기 선택 완료
    CombatStart,// 적이 문 부수고 나옴 및 전투 튜토리얼 시작
    GetItem,
    DialogeNShowCrunchKey, //웅크리기 키 튜토리얼
    ApperBear, // 흉포한 곰 등장 
    CollapseWall, // 벽이 무너지거나 부셔져서 길이 막히는 이벤트
    TutorialEnd// 임시기지로 쓰일 수 있는 안전지대 도착
}



public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialTriggerType triggerType;

    private void OnTriggerEnter(Collider other)
    {
        TutorialManager.Instance.OnTrigger(triggerType);
    }
}