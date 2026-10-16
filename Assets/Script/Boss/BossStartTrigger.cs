using UnityEngine;

// 사용법: 전투 시작 트리거 GameObject에 BoxCollider(isTrigger=true)와 함께 붙임
// 플레이어 진입 시 보스에 타겟 전달 후 비활성화
// 전투구역(BossCombatZone)에서 플레이어가 이탈하면 다시 활성화됨
public class BossStartTrigger : MonoBehaviour
{
    [SerializeField] private BossStateMachine boss;

    private void OnTriggerEnter(Collider other)
    {
        boss.OnPlayerEnterZone(other.GetComponent<PlayerStateMachine>());
        gameObject.SetActive(false);
    }
}
