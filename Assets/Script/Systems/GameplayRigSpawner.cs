using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// CoreScene의 [Core] 아래에 붙음. 플레이어·카메라·HUD 묶음(GameplayRig 프리팹)을
//   - 게임 씬에 도착하면 Core 씬 안에 만들고 (이미 있으면 그대로 → 구역을 오가도 유지)
//   - 메뉴 씬에 도착하면 지움 (다음 판은 새로 만들어서 깨끗하게 시작)
// 그리고 도착한 씬의 PlayerSpawnPoint로 플레이어를 옮김.
//
// CoreRoot(Core 어셈블리)는 플레이어를 모르므로 이 일은 Game 쪽인 여기서 함.
public class GameplayRigSpawner : MonoBehaviour
{
    [SerializeField] private GameObject gameplayRigPrefab;
    [Tooltip("이 씬들에 도착하면 리그를 지움")]
    [SerializeField] private string[] menuScenes = { "StartMenuScene" };

    private GameObject rig;

    void OnEnable() => SceneFlow.Arrived += OnArrived;
    void OnDisable() => SceneFlow.Arrived -= OnArrived;

    void OnArrived(Scene scene)
    {
        if (Array.IndexOf(menuScenes, scene.name) >= 0)
        {
            if (rig != null) { Destroy(rig); rig = null; }
            return;
        }

        // 옛 데모 씬처럼 씬 안에 플레이어가 이미 있으면 리그를 만들지 않음 (플레이어 두 명 방지)
        if (rig == null && !SceneHasOwnPlayer(scene))
        {
            if (gameplayRigPrefab == null) { Debug.LogError("[GameplayRigSpawner] gameplayRigPrefab이 비어 있음"); return; }
            rig = Instantiate(gameplayRigPrefab);
            rig.name = gameplayRigPrefab.name;
            SceneManager.MoveGameObjectToScene(rig, gameObject.scene);   // Core 씬 안에 → 콘텐츠 씬이 바뀌어도 유지
        }

        MovePlayerToSpawn(scene);
    }

    static bool SceneHasOwnPlayer(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
            if (root.GetComponentInChildren<PlayerStateMachine>(true) != null) return true;
        return false;
    }

    static void MovePlayerToSpawn(Scene scene)
    {
        var spawn = PlayerSpawnPoint.FindIn(scene);
        var player = PlayerStateMachine.Instance;
        if (spawn == null || player == null || player.gameObject.scene == scene) return;   // 씬 자체 플레이어면 그대로

        player.Rb.position = spawn.transform.position;
        player.transform.position = spawn.transform.position;
        player.Rb.linearVelocity = Vector3.zero;
    }
}
