using System.Collections;
using UnityEngine;

// 몬스터 디버그 도구 (상태 로그 + 범위 기즈모). 보고 싶은 몬스터에만 붙임.
// 클래스·직렬화 필드는 #if 밖 (빌드에서 "missing script"·직렬화 불일치 방지), 동작만 에디터 전용
[RequireComponent(typeof(MonsterStateMachine))]
public class MonsterDebug : MonoBehaviour
{
#pragma warning disable CS0414   // 빌드에서는 안 읽혀서 나는 "할당만 됨" 경고 끄기
    [SerializeField] private bool logStatus = false;
    [SerializeField] private float logInterval = 0.5f;
    [SerializeField] private bool drawGizmos = true;
#pragma warning restore CS0414

#if UNITY_EDITOR
    private MonsterStateMachine monster;

    void Start()
    {
        monster = GetComponent<MonsterStateMachine>();
        if (logStatus) StartCoroutine(DebugStatusCoroutine());
    }

    IEnumerator DebugStatusCoroutine()
    {
        var wait = new WaitForSeconds(logInterval);
        while (true)
        {
            yield return wait;

            var target = monster.Targetplayer;
            var status = monster.status;
            var rb = monster.Rb;
            bool isBoss = monster is BossStateMachine;

            string targetName = target != null ? target.name : "없음";
            float dist = target != null
                ? Vector3.Distance(transform.position, target.transform.position)
                : -1f;

            string text =
                $"[{(isBoss ? "BOSS" : "MONSTER")} DEBUG] {gameObject.name}\n" +
                $"  ActiveState     : {monster.ActiveState?.GetType().Name ?? "null"}\n" +
                $"  HP              : {status.Hp:F0} / {status.Maxhp}\n";

            if (monster is BossStateMachine boss)
            {
                text +=
                    $"  Target          : {targetName}\n" +
                    $"  TargetDist      : {(dist >= 0 ? dist.ToString("F1") : "N/A")}\n" +
                    $"  BattleRange     : {status.battleHalfExtents}\n" +
                    $"  IsFirstAttack   : {boss.IsFirstAttack}\n" +
                    $"  IsSpecialReady  : {boss.IsSpecialReady}\n" +
                    $"  --- 물리 ---\n" +
                    $"  Velocity        : {rb.linearVelocity}\n" +
                    $"  Position        : {rb.position}\n" +
                    $"  Rotation(Y)     : {rb.rotation.eulerAngles.y:F1}";
            }
            else
            {
                float brighten = target != null ? target.status.currentbrighten : -1f;
                bool hasLOS = target != null && !Physics.Linecast(transform.position, target.transform.position, monster.ObstacleMask);
                text +=
                    $"  DetectionGauge  : {status.detection_gauge:F1} / 100\n" +
                    $"  Target          : {targetName}\n" +
                    $"  TargetDist      : {(dist >= 0 ? dist.ToString("F1") : "N/A")}\n" +
                    $"  CurBrighten     : {(brighten >= 0 ? brighten.ToString("F3") : "N/A")}\n" +
                    $"  HasLOS          : {(target != null ? hasLOS.ToString() : "N/A")}\n" +
                    $"  DetectRange     : {status.detectHalfExtents}\n" +
                    $"  BattleRange     : {status.battleHalfExtents}\n" +
                    $"  --- 물리 ---\n" +
                    $"  Velocity        : {rb.linearVelocity}\n" +
                    $"  Position        : {rb.position}\n" +
                    $"  IsKinematic     : {rb.isKinematic}\n" +
                    $"  Constraints     : {rb.constraints}\n" +
                    $"  Rotation(Y)     : {rb.rotation.eulerAngles.y:F1}";
            }
            Debug.Log(text);
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        // 플레이 전에도 불리므로 Start에서 채운 monster 대신 바로 가져옴
        var m = monster != null ? monster : GetComponent<MonsterStateMachine>();
        if (m == null || m.status == null) return;

        // 감지 범위 (노랑) — 보스는 감지가 없어서 안 그림 (기존과 같음)
        if (m is not BossStateMachine)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, m.status.detectHalfExtents * 2f);
        }
        // 전투 범위 (빨강)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, m.status.battleHalfExtents * 2f);
        // 공격 범위 (마젠타)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, m.status.atk_range);
    }
#endif
}
