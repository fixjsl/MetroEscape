#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// 메타 진행 저장 파일을 다루는 에디터 메뉴.
// 저장 위치: %USERPROFILE%\AppData\LocalLow\DefaultCompany\SubwayExit\meta_progress.json
public static class MetaProgressEditor
{
    private const string Root = "Tools/메타 진행/";

    [MenuItem(Root + "해금 기록 초기화", priority = 0)]
    private static void ResetProgress()
    {
        if (!EditorUtility.DisplayDialog(
                "해금 기록 초기화",
                "해금한 퍽 기록을 전부 지웁니다.\n되돌릴 수 없습니다. 진행하시겠습니까?",
                "초기화", "취소"))
            return;

        if (Application.isPlaying && MetaProgressManager.Instance != null)
        {
            MetaProgressManager.Instance.ResetProgress();
            Debug.Log("[MetaProgress] 실행 중 초기화 완료. 퍽 선택 화면을 다시 열면 전부 잠겨 있습니다.");
            return;
        }

        string path = MetaProgressManager.SavePath;
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[MetaProgress] 저장 파일 삭제 완료\n{path}");
        }
        else
        {
            Debug.Log($"[MetaProgress] 저장 파일이 이미 없습니다\n{path}");
        }
    }

    [MenuItem(Root + "전부 해금", priority = 1)]
    private static void UnlockAll()
    {
        var progress = new MetaProgress();

        foreach (string guid in AssetDatabase.FindAssets("t:PerkBase"))
        {
            var perk = AssetDatabase.LoadAssetAtPath<PerkBase>(AssetDatabase.GUIDToAssetPath(guid));
            if (perk != null && !progress.unlockedPerkIds.Contains(perk.PerkId))
                progress.unlockedPerkIds.Add(perk.PerkId);
        }

        JsonFileStore.Save(MetaProgressManager.SavePath, progress);

        string note = Application.isPlaying
            ? " 실행 중이므로 다음 재생부터 반영됩니다."
            : "";
        Debug.Log($"[MetaProgress] 퍽 {progress.unlockedPerkIds.Count}개 해금 기록 작성.{note}");
    }

    [MenuItem(Root + "저장 폴더 열기", priority = 20)]
    private static void OpenSaveFolder()
    {
        Directory.CreateDirectory(Application.persistentDataPath);
        EditorUtility.RevealInFinder(MetaProgressManager.SavePath);
    }

    [MenuItem(Root + "저장 경로 로그로 출력", priority = 21)]
    private static void LogSavePath()
    {
        Debug.Log($"[MetaProgress] {MetaProgressManager.SavePath}");
    }
}
#endif
