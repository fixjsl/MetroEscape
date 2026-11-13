using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Core 씬 위에 콘텐츠 씬을 Additive로 올리고 내리는 창구.
// CoreScene은 절대 내리지 않음 → 매니저들은 계속 살아 있음.
//
//   Go("Basecamp")          화면 전체 전환 (메뉴 → 기지, 게임오버 → 메뉴): Core 빼고 전부 내리고 대상 씬을 올려 활성 씬으로
//   LoadAdditive("Zone1")   이어 붙이기 (구역 스트리밍): 지금 씬은 그대로 두고 하나 더 올림
//   Unload("Zone1")         멀어진 구역 내리기
//
// 씬 이름 또는 경로("Assets/Scenes/X.unity") 둘 다 받음.
// 에디터에서는 Build Settings에 없는 씬도 경로로 불러올 수 있음 (빌드에서는 Build Settings에 있어야 함)
public static class SceneFlow
{
    public const string CoreSceneName = "CoreScene";

    public static bool IsBusy { get; private set; }
    public static event Action<Scene> SceneLoaded;      // 콘텐츠 씬 하나가 다 올라온 뒤
    public static event Action<string> SceneUnloaded;   // 콘텐츠 씬 하나가 내려간 뒤
    public static event Action<Scene> Arrived;          // Go로 화면 전환이 끝난 뒤 (도착한 활성 씬) — 플레이어 생성·스폰 위치 등

    public static void Go(string scene, Action done = null) => Run(GoRoutine(scene, done));
    public static void LoadAdditive(string scene, Action done = null) => Run(LoadRoutine(scene, false, done));
    public static void Unload(string scene, Action done = null) => Run(UnloadRoutine(scene, done));

    public static bool IsLoaded(string scene) => Find(scene).isLoaded;

    // ---------------------------------------------------------------

    static IEnumerator GoRoutine(string scene, Action done)
    {
        IsBusy = true;
        // Core 빼고 전부 내림
        var toUnload = new List<Scene>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name != CoreSceneName) toUnload.Add(s);
        }
        foreach (var s in toUnload)
        {
            string name = s.name;
            var op = SceneManager.UnloadSceneAsync(s);
            if (op != null) yield return op;
            SceneUnloaded?.Invoke(name);
        }
        yield return LoadRoutine(scene, true, null);
        IsBusy = false;
        Arrived?.Invoke(SceneManager.GetActiveScene());
        done?.Invoke();
    }

    static IEnumerator LoadRoutine(string scene, bool makeActive, Action done)
    {
        if (!Find(scene).isLoaded)
        {
            var op = LoadOp(scene);
            if (op == null) { Debug.LogError($"[SceneFlow] 씬을 불러올 수 없음: {scene} (Build Settings 확인)"); yield break; }
            yield return op;
        }
        var loaded = Find(scene);
        if (makeActive && loaded.IsValid()) SceneManager.SetActiveScene(loaded);   // 새로 만드는 오브젝트·조명 기준 씬
        SceneLoaded?.Invoke(loaded);
        done?.Invoke();
    }

    static IEnumerator UnloadRoutine(string scene, Action done)
    {
        var s = Find(scene);
        if (s.isLoaded && s.name != CoreSceneName)
        {
            var op = SceneManager.UnloadSceneAsync(s);
            if (op != null) yield return op;
            SceneUnloaded?.Invoke(scene);
        }
        done?.Invoke();
    }

    static AsyncOperation LoadOp(string scene)
    {
        var param = new LoadSceneParameters(LoadSceneMode.Additive);
#if UNITY_EDITOR
        // 에디터: Build Settings에 없는 씬도 경로로 불러오기
        string path = scene.EndsWith(".unity") ? scene : FindPathInProject(scene);
        if (path != null && SceneUtility.GetBuildIndexByScenePath(path) < 0)
            return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, param);
#endif
        return SceneManager.LoadSceneAsync(scene, param);
    }

    static Scene Find(string scene)
    {
        var s = scene.EndsWith(".unity") ? SceneManager.GetSceneByPath(scene) : SceneManager.GetSceneByName(scene);
        return s;
    }

#if UNITY_EDITOR
    static string FindPathInProject(string sceneName)
    {
        foreach (var guid in UnityEditor.AssetDatabase.FindAssets($"t:Scene {sceneName}"))
        {
            var p = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(p) == sceneName) return p;
        }
        return null;
    }
#endif

    static void Run(IEnumerator routine)
    {
        if (CoreRoot.Instance == null) { Debug.LogError("[SceneFlow] CoreRoot가 없음 — CoreScene부터 시작해야 함"); return; }
        CoreRoot.Instance.StartCoroutine(routine);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { IsBusy = false; SceneLoaded = null; SceneUnloaded = null; Arrived = null; }
}
