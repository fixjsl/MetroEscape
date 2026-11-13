using UnityEditor;
using UnityEditor.SceneManagement;

// 에디터에서 어떤 씬을 열고 Play를 눌러도 CoreScene(매니저)부터 시작하게 함.
// 원래 열려 있던 씬 경로는 SessionState로 넘기고, CoreRoot.Start가 그 씬을 불러옴.
// 빌드에는 영향 없음 (빌드는 Build Settings 0번 = CoreScene).
[InitializeOnLoad]
static class CorePlayModeStart
{
    static CorePlayModeStart()
    {
        Apply();
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void Apply()
    {
        var core = AssetDatabase.LoadAssetAtPath<SceneAsset>(CoreRoot.CoreScenePath);
        EditorSceneManager.playModeStartScene = core;   // 없으면 null → 평소처럼 열린 씬에서 시작
    }

    static void OnPlayModeChanged(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.ExitingEditMode) return;
        Apply();
        // 지금 열려 있는(활성) 씬을 기억 → Core가 올라온 뒤 이 씬으로 넘어감
        SessionState.SetString(CoreRoot.EditorStartSceneKey, EditorSceneManager.GetActiveScene().path);
    }

    [MenuItem("MetroEscape/Play From Opened Scene Without Core")]
    static void PlayWithoutCore()
    {
        // 필요할 때만: Core 없이 지금 씬 그대로 테스트
        EditorSceneManager.playModeStartScene = null;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.EnterPlaymode();
        EditorApplication.playModeStateChanged += RestoreAfterPlay;
    }

    static void RestoreAfterPlay(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredEditMode) return;
        EditorApplication.playModeStateChanged -= RestoreAfterPlay;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        Apply();
    }
}
