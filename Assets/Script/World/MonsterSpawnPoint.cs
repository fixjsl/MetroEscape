using UnityEngine;

public class MonsterSpawnPoint : MonoBehaviour
{
    [SerializeField] private MonsterStateMachine prefab;
    public MonsterStateMachine Prefab => prefab;
    public MonsterStateMachine Current { get; set; }   // MonsterManager가 채움

    void OnEnable() { MonsterManager.AddPoint(this); }
    void OnDisable() { MonsterManager.RemovePoint(this); }
    // 에디터: OnDrawGizmos로 위치 표시 (씬 뷰에서 보이게)
}