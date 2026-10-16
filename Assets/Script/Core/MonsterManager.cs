using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public enum MonsterType {}
public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;
    // Ȱ��ȭ�� ���� ����Ʈ
    private List<MonsterStateMachine> DiedMonsters = new List<MonsterStateMachine>();
    private List<BossStateMachine> DiedBosses = new List<BossStateMachine>();

    // ��Ȱ��ȭ�� ���� Ǯ (������ Ÿ�Ժ�)
    private Dictionary<MonsterType, Queue<MonsterStateMachine>> pool = new Dictionary<MonsterType, Queue<MonsterStateMachine>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void Preload(MonsterType type, GameObject prefab, int count, Transform parent = null)
    {
        if (!pool.ContainsKey(type))
            pool[type] = new Queue<MonsterStateMachine>();

        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab, parent);
            var monster = obj.GetComponent<MonsterStateMachine>();
            obj.SetActive(false);
            pool[type].Enqueue(monster);
        }
    }
    public void Register(MonsterStateMachine monster)
    {
        if (!DiedMonsters.Contains(monster))
            DiedMonsters.Add(monster);
    }

    // ���� ��� �� Ǯ�� ��ȯ
    public void ReturnToPool(MonsterStateMachine monster, MonsterType key)
    {
        DiedMonsters.Remove(monster);
        monster.gameObject.SetActive(false);

        if (!pool.ContainsKey(key))
            pool[key] = new Queue<MonsterStateMachine>();

        pool[key].Enqueue(monster);
    }

}
