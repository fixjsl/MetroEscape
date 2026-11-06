using UnityEngine;

// 몬스터 머리 위 UI를 만들고 몬스터 이벤트에 연결 (MonsterStateMachine은 UI를 모름)
[RequireComponent(typeof(MonsterStateMachine))]
public class MonsterUIBinder : MonoBehaviour
{
    [SerializeField] private MonsterUI monsterUIPrefab;
    [SerializeField] private Transform hpUIPoint;
    [SerializeField] private Transform detectionUIPoint;

    private MonsterStateMachine monster;
    private MonsterUI monsterUI;

    // Awake가 아니라 Start: MonsterStateMachine.Awake의 status = Instantiate(status)가 끝난 뒤에 구독해야 함
    void Start()
    {
        if (monsterUIPrefab == null) return;
        monster = GetComponent<MonsterStateMachine>();
        monsterUI = Instantiate(monsterUIPrefab);
        monsterUI.Init(monster, hpUIPoint, detectionUIPoint);
        monster.Hidden += OnHidden;
        monster.Respawned += monsterUI.ResetForRespawn;
    }

    // 씬 종료 때는 UI가 먼저 파괴될 수 있음 (MonsterSpawnPoint.OnDisable → Despawn → HideUI)
    void OnHidden() { if (monsterUI != null) monsterUI.gameObject.SetActive(false); }

    void OnDestroy()
    {
        if (monster == null || monsterUI == null) return;
        monster.Hidden -= OnHidden;
        monster.Respawned -= monsterUI.ResetForRespawn;
    }
}
