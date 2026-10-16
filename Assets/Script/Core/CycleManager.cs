using System.Collections.Generic;
using UnityEngine;

public class CycleManager : MonoBehaviour
{
    public static CycleManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [SerializeField] private Transform playerBasePosition;
    [SerializeField] private BaseDepartureGate departureGate;
    // 씬에 빈 오브젝트로 배치한 비상구 등장 가능 위치들
    [SerializeField] private ZoneEntry[] zones;
    public IReadOnlyList<ZoneEntry> Zones => zones;
    void Awake()
    {
        Instance = this;
    }

    public void ExecuteCycleReset()
    {
        TeleportPlayerToBase();
        RespawnMonsters();
        ResetContainers();
        LootTable.ResetUniquesForCycle();
        HideAllExits();
        if (departureGate != null) departureGate.ResetForCycle();
    }
    public List<ZoneEntry> GetOpenZones()
    {
        var result = new List<ZoneEntry>();
        foreach (var z in zones) if (z.IsOpen) result.Add(z);
        return result;
    }   

    // BaseDepartureGate에서 시작지점 선택 후 호출 — 가장 먼 위치에 비상구 배치
    public void PlaceExits(ZoneEntry startzone,Vector3 Startpos)
    {
        foreach (var z in zones)
        {
            if(z.exitPrefab == null) continue;
            if(!z.IsOpen || z.exitPoints== null || z.exitPoints.Length == 0)
            {
                z.exitPrefab.gameObject.SetActive(false);
                continue;
            } 
            Transform chosen;
            if(z==startzone)
            {
                chosen = z.exitPoints[0];
                float best = -1f;
                foreach(var p in z.exitPoints)
                {
                    float d = Vector3.Distance(Startpos, p.position);
                    if(d>best) { best = d; chosen = p; }
                }
            }
            else
            {
                chosen = z.exitPoints[Random.Range(0,z.exitPoints.Length)];
            }
             z.exitPrefab.gameObject.SetActive(true);
             z.exitPrefab.transform.position = chosen.position;
             z.exitPrefab.ResetForCycle();
        }
       

    }
    public Vector3 PickStartPoint(ZoneEntry zone, float spawnZ)
    {
        Vector3 raw = zone.startPoints[Random.Range(0, zone.startPoints.Length)].position;
        return new Vector3(raw.x, raw.y, spawnZ);
    }
    public void HideAllExits()
    {
        foreach(var z in zones)
        {
            if(z.exitPrefab != null) z.exitPrefab.gameObject.SetActive(false);
        }
    }

    private void TeleportPlayerToBase()
    {
        if (playerBasePosition == null) return;
        var player = PlayerStateMachine.Instance;
        if (player == null) return;
        player.Rb.position = playerBasePosition.position;
        player.Rb.linearVelocity = Vector3.zero;
    }

    private void RespawnMonsters()
    {
        // 컴포넌트가 disabled된 몬스터도 포함해서 검색, 보스는 제외
        var monsters = FindObjectsByType<MonsterStateMachine>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var m in monsters)
        {
            if (m is BossStateMachine) continue;
            m.CycleRespawn();
        }
    }

    private void ResetContainers()
    {
        // SetActive(false)로 숨겨진 컨테이너도 포함
        var containers = FindObjectsByType<ContainerObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in containers)
            c.CycleReset();
    }

}
