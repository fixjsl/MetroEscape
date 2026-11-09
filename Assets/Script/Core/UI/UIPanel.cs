using UnityEngine;
using UnityEngine.Serialization;

public abstract class UIPanel<T> : UIPanel where T : UIPanel<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = (T)this;
    }
    protected virtual void OnDestroy() { if (Instance == this) Instance = null; }
}

public abstract class UIPanel : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("panel"), FormerlySerializedAs("pausePanel")] 
    protected GameObject root;         // 켜고 끌 패널 (기존 panel 필드)
    [SerializeField] bool blocksCombatInput = true;
    [SerializeField] bool pausesTime = false;
    [SerializeField] bool showsCursor = true;
    [SerializeField] bool closeOnCancel = true;          // 게임오버/클리어는 false

    public bool IsOpen { get; private set; }
    // 위 설정들의 읽기 전용 프로퍼티 (매니저가 읽음)
    public bool BlocksCombatInput => blocksCombatInput;   // ← 매니저는 이걸 읽음
    public bool PausesTime => pausesTime;
    public bool ShowsCursor => showsCursor;
    public bool CloseOnCancel => closeOnCancel;

    public void Open() {
        /* 이미 열려 있으면 return → root 켜기 → Push → OnOpened */
        if (IsOpen) return;
        IsOpen = true;
        root.SetActive(true);
        UIStackManager.Push(this);
        OnOpened();
    }
    public void Close() {
        /* 닫혀 있으면 return → root 끄기 → Pop → OnClosed */
        if (!IsOpen) return;
        IsOpen = false;
        root.SetActive(false);
        UIStackManager.Pop(this);
        OnClosed();
    }
    protected virtual void OnOpened() { }   // 자식: 목록 다시 그리기 등
    protected virtual void OnClosed() { }   // 자식: 선택 해제 등
}
