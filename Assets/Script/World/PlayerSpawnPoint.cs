using UnityEngine;
using UnityEngine.SceneManagement;

// 콘텐츠 씬(기지·구역)에 하나 두는 "플레이어가 여기서 시작" 표시.
// SceneFlow.Go로 이 씬에 도착하면 GameplayRigSpawner가 플레이어를 이 위치로 옮김.
public class PlayerSpawnPoint : MonoBehaviour
{
    // 그 씬 안의 첫 스폰 지점 (없으면 null)
    public static PlayerSpawnPoint FindIn(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var p = root.GetComponentInChildren<PlayerSpawnPoint>(true);
            if (p != null) return p;
        }
        return null;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(0.6f, 2f, 0.6f));
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + transform.right * 0.8f);
    }
#endif
}
