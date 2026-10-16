using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerkSelectUI : MonoBehaviour
{
    public static PerkSelectUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [SerializeField] private GameObject panel;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private PerkButton buttonPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private bool hideLockedName = true;

    private readonly List<PerkBase> selected = new List<PerkBase>();
    private readonly Dictionary<PerkBase, Image> buttonImages = new Dictionary<PerkBase, Image>();
    private Action<IReadOnlyList<PerkBase>> onConfirmed;
    private Action onCancelled;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        confirmButton.onClick.AddListener(Confirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(Cancel);
    }

    public void Show(Action<IReadOnlyList<PerkBase>> callback, Action onCancel = null)
    {
        // 설정이 잘못된 경우에는 취소가 아니라 '퍽 없이 진행'으로 빠진다.
        // 취소로 처리하면 시작 메뉴로 되돌아가 영영 시작할 수 없게 된다.
        if (buttonPrefab == null)
        {
            Debug.LogError("[PerkSelectUI] buttonPrefab 없음");
            callback?.Invoke(null);
            return;
        }

        var mgr = MetaProgressManager.Instance;
        if (mgr == null)
        {
            Debug.LogError("[PerkSelectUI] MetaProgressManager 없음");
            callback?.Invoke(null);
            return;
        }

        onConfirmed = callback;
        onCancelled = onCancel;
        selected.Clear();
        buttonImages.Clear();

        for (int i = buttonContainer.childCount - 1; i >= 0; i--)
            Destroy(buttonContainer.GetChild(i).gameObject);

        foreach (var item in mgr.AllPerks)
        {
            if (item == null) continue;

            var perk = item;                       // 클로저 캡처용 지역 변수
            bool unlocked = mgr.IsUnlocked(perk);

            var pb = Instantiate(buttonPrefab, buttonContainer);
            pb.gameObject.SetActive(true);

            pb.button.interactable = unlocked;
            pb.icon.sprite = unlocked ? perk.icon : lockedSprite;
            pb.label.text  = unlocked
                ? $"<b>{perk.PerkName}</b>\n<size=75%><color=#B9C0CC>{perk.description}</color></size>"
                : $"<b><color=#7A828F>{(hideLockedName ? "???" : perk.PerkName)}</color></b>\n" +
                  $"<size=75%><color=#D8B24A>{perk.GetConditionText()}</color></size>";

            buttonImages[perk] = pb.background;

            if (unlocked) pb.button.onClick.AddListener(() => Toggle(perk));
        }

        panel.SetActive(true);
    }

    private void Toggle(PerkBase perk)
    {
        if (selected.Contains(perk)) selected.Remove(perk);
        else if (selected.Count < 2) selected.Add(perk);
        else return;                               // 2개 초과는 무시

        foreach (var pair in buttonImages)
            if (pair.Value != null)
                pair.Value.color = selected.Contains(pair.Key) ? selectedColor : Color.white;
    }

    private void Confirm()
    {
        var cb = onConfirmed;                      // 콜백을 먼저 붙잡아 둘 것
        var result = new List<PerkBase>(selected);

        Close();

        cb?.Invoke(result);
    }

    private void Cancel()
    {
        var cb = onCancelled;                      // 콜백을 먼저 붙잡아 둘 것

        Close();

        cb?.Invoke();
    }

    // 패널을 닫고 상태를 비운다. 콜백을 부르기 전에 호출할 것.
    private void Close()
    {
        panel.SetActive(false);
        selected.Clear();
        onConfirmed = null;
        onCancelled = null;
    }
}
