using System.Collections.Generic;
using System.IO;
using UnityEngine;



public class MetaProgressManager : MonoBehaviour
{
    public static MetaProgressManager Instance { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;
    [SerializeField] private PerkBase[] allPerks;

    public MetaProgress progress { get; private set; } = new MetaProgress();
    private readonly List<PerkBase> pending = new List<PerkBase>();
    public int killCount { get; private set; }
    public int runCount {get; private set; }
    private readonly Dictionary<int, int> gathered = new Dictionary<int, int>();

    public int GatherKinds => gathered.Count;
    public int GatherCountOf(int itemcode) => gathered.TryGetValue(itemcode, out int n) ? n : 0;

    public const string SaveFileName = "meta_progress.json";
    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public IReadOnlyList<PerkBase> AllPerks => allPerks;
    public bool IsUnlocked(PerkBase p) => p != null && progress.unlockedPerkIds.Contains(p.PerkId);

    void Awake()
    {
        if(Instance == null) {Instance = this; DontDestroyOnLoad(gameObject);}
        else { Destroy(gameObject); return; }
        Load();
    }
    void Load()
    {
        if(File.Exists(SavePath))
            JsonUtility.FromJsonOverwrite(File.ReadAllText(SavePath), progress);
    }

    public void Save() => File.WriteAllText(SavePath, JsonUtility.ToJson(progress));

    

    // 퍽 선택 보관 및 적용
    public void SetPending(IReadOnlyList<PerkBase> perks)
    {
        pending.Clear();
        if (perks != null) pending.AddRange(perks);
    }

    public void ApplyPending(PlayerStatus status, Inventory inventory)
    {
        foreach (var p in pending)
            if (p != null) p.Apply(status, inventory);
    }
    //세션 경계

    public void BeginSession()
    {
        killCount = 0;
        gathered.Clear();
        runCount = 0;
    }
    // 세션 지표
    public void AddKill() => killCount++;

    public void AddRun() => runCount++;

    public void AddGather(ItemBase item, int n)
    {
        if (item == null || n <= 0) return;
        gathered.TryGetValue(item.itemcode, out int cur);
        gathered[item.itemcode] = cur + n;
    }    
    // 해금 기록을 지우고 저장 파일도 삭제한다. 에디터 메뉴와 디버그용.
    public void ResetProgress()
    {
        progress.unlockedPerkIds.Clear();
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }

    //해금
    //게임 오버시 호출, 새로 해금된 퍽 목록을 돌려준다.
    public List<PerkBase> EvaluateUnlocks()
    {
        var newly = new List<PerkBase>();
        foreach(var p in allPerks)
        {
            if(p== null) continue;
            if(progress.unlockedPerkIds.Contains(p.PerkId)) continue;

            int value = p.axis switch
            {
                PerkAxis.Combat => killCount,
                PerkAxis.Gather  => p.targetItem != null
                                    ? GatherCountOf(p.targetItem.itemcode)
                                    : GatherKinds,  
                PerkAxis.Explore => runCount,
                _ => 0
            };

            if (value >= p.threshold)
            {
                progress.unlockedPerkIds.Add(p.PerkId);
                newly.Add(p);
            }
        }
        if (newly.Count > 0) Save();
        return newly;
    }

}