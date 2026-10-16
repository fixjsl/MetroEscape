using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ZoneSelectUI : MonoBehaviour
{
    public static ZoneSelectUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [SerializeField] private GameObject panel;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Button cancelButton;

    private Action<ZoneEntry> onSelescted;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        cancelButton.onClick.AddListener(Hide);
    }

    public void Show(IReadOnlyList<ZoneEntry> zones, Action<ZoneEntry> callback)
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("[ZoneSelectUI] buttonPrefab이 비어 있거나 파괴되었습니다.");
            return;
        }
        onSelescted = callback;

        foreach (Transform child in buttonContainer) Destroy(child.gameObject);

        foreach (var z in zones)
        {
            var zone = z;                              // 클로저 캡처 주의
            var btn = Instantiate(buttonPrefab, buttonContainer);
            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = zone.zoneName;
            btn.onClick.AddListener(() => {    
                 var cb = onSelescted;
                Hide();
                cb?.Invoke(zone);});
            }

        panel.SetActive(true);
    }

    private void Hide()
    {
        panel.SetActive(false);
        onSelescted = null;
    }
}