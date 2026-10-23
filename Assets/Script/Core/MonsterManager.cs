using System.Collections.Generic;
using UnityEngine;

public static class MonsterManager 
{
    static readonly Dictionary<MonsterStateMachine, Stack<MonsterStateMachine>> pools = new();
    static readonly HashSet<MonsterSpawnPoint> points = new();
    static readonly HashSet<MonsterStateMachine> placed = new();   // 풀 밖에서 씬에 직접 놓인 몬스터 (옮기는 동안 + 튜토리얼 늑대)
    static Transform root;                                          // 꺼진 몬스터를 모아 둘 부모 (없으면 만듦)
    private static Transform Root
    {
        get
        {
            if (root == null) root = new GameObject("[MonsterPool]").transform;
            return root;
        }
    }
    private static MonsterStateMachine Take(MonsterStateMachine prefeb)
    {
        if(pools.TryGetValue(prefeb, out var stack))
        {
            while (stack.Count > 0)
            {
                var m = stack.Pop();
                if (m != null) return m;
            }
        }
        var created = Object.Instantiate(prefeb, Root);
        Unregister(created);
        return created;
    }
    public static void AddPoint(MonsterSpawnPoint p) { points.Add(p); Spawn(p); }    // points.Add + Spawn(p)
    public static void RemovePoint(MonsterSpawnPoint p) { Despawn(p); points.Remove(p); }  // Despawn(p) + points.Remove
    public static void Spawn(MonsterSpawnPoint p)
    {
        var m = Take(p.Prefab);
        m.transform.SetParent(null);
        m.spawnpoint = p.transform;
        m.CycleRespawn();
        p.Current = m;
    }         // Take(p.Prefab) → 위치·spawnpoint 지정 → CycleRespawn() → p.Current
    public static void Despawn(MonsterSpawnPoint p)
    {
        var m = p.Current;
        if (m == null) return;

        m.HideUI();
        m.gameObject.SetActive(false);
        m.transform.SetParent(root);
        if (!pools.TryGetValue(p.Prefab, out var s)) pools[p.Prefab] = s = new Stack<MonsterStateMachine>(); s.Push(m);
        p.Current = null;
    }// p.Current 끄고 root 아래로 → pools[원본]에 Push → p.Current = null
    public static void ResetAll()
    { 
        foreach(var p in points)
        {
            Despawn(p);
            Spawn(p);
        }
        foreach(var m in placed)
        {
            if(m != null)
                m.CycleRespawn();
        }
    }// points 복사본마다 Despawn → Spawn, placed는 CycleRespawn

    public static void Register(MonsterStateMachine m)
    {
        if (m.RespawnsOnCycle) placed.Add(m);
    }// Awake에서: RespawnsOnCycle이면 placed.Add
    public static void Unregister(MonsterStateMachine m)
    {
        placed.Remove(m);
    }// OnDestroy에서: placed.Remove

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { pools.Clear(); points.Clear(); placed.Clear();  }
}
