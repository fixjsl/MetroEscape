using UnityEngine;
using BossStates;

// 전투 구역 경계 트리거
// - 플레이어 진입: 보스가 귀환 중이면 즉시 전투 재개
// - 플레이어 이탈: 전투 종료 (시작 트리거는 보스가 스폰포인트 도착 시 재활성화)
public class BossCombatZone : MonoBehaviour
{
    [SerializeField] private BossStateMachine boss;

    private void OnTriggerEnter(Collider other)
    {
        if (boss.ActiveState is BossReturn)
            boss.OnPlayerEnterZone(other.GetComponent<PlayerStateMachine>());
    }

    private void OnTriggerExit(Collider other)
    {
        boss.OnPlayerExitZone();
    }
}
