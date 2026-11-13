using UnityEngine;
using UnityEngine.SceneManagement;

// CoreScene의 루트. 게임에서 가장 먼저 올라오고(Build Settings 0번) 끝까지 안 내려감.
// 콘텐츠 씬(시작 메뉴, 기지, 구역들)은 SceneFlow로 Core 위에 Additive로 올리고 내림.
//
// - 매니저는 이 오브젝트 아래에만 둠 → 씬마다 복사본 둘 필요 없음
// - 콘텐츠 씬에 옛 매니저 복사본이 남아 있어도 각 매니저의 SingletonBehaviour가 늦게 온 쪽을 지움
// - 에디터에서는 CorePlayModeStart(에디터 스크립트)가 Play 때 항상 이 씬부터 시작하게 하고,
//   원래 열려 있던 씬 경로를 넘겨줌 → 아무 씬에서 Play해도 Core 위에 그 씬이 올라옴
public class CoreRoot : SingletonBehaviour<CoreRoot>
{
    public const string CoreScenePath = "Assets/Scenes/CoreScene.unity";
    public const string EditorStartSceneKey = "MetroEscape.PlayStartScene";

    [Tooltip("Core 다음에 처음 올릴 씬 (빌드에서는 Build Settings에 있어야 함)")]
    [SerializeField] private string firstScene = "StartMenuScene";

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        SceneManager.sceneLoaded += RemoveDuplicateEventSystems;
    }

    protected override void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= RemoveDuplicateEventSystems;
        base.OnDestroy();
    }

    void Start()
    {
        string next = firstScene;
#if UNITY_EDITOR
        // 에디터에서 Play를 누른 씬이 있으면 그 씬부터 (Core 자체에서 눌렀으면 firstScene)
        string requested = UnityEditor.SessionState.GetString(EditorStartSceneKey, "");
        UnityEditor.SessionState.EraseString(EditorStartSceneKey);
        if (!string.IsNullOrEmpty(requested) && requested != CoreScenePath) next = requested;
#endif
        SceneFlow.Go(next);
    }

    // 콘텐츠 씬에 옛 EventSystem 복사본이 남아 있으면 지움 (Core 것 하나만)
    // EventSystem은 싱글턴이 아니라 둘이면 경고가 계속 뜸. 씬에서 지우고 나면 이 코드는 아무 일도 안 함
    void RemoveDuplicateEventSystems(Scene scene, LoadSceneMode mode)
    {
        foreach (var root in scene.GetRootGameObjects())
            foreach (var es in root.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true))
                if (!es.transform.IsChildOf(transform)) Destroy(es.gameObject);
    }
}
