using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("Audio")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambientSlider;

    private static readonly (int w, int h)[] commonResolutions =
    {
        (1280,  720),
        (1600,  900),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160),
    };

    private readonly List<(int w, int h)> supportedResolutions = new List<(int w, int h)>();

    void Awake()
    {
        InitResolutions();
        InitWindowMode();
        InitQuality();
        InitAudio();
        //설정 종료 버튼

    }

    // ===================== 그래픽 =====================

    void InitResolutions()
    {
        // 모니터가 지원하는 해상도 목록
        var screenResolutions = Screen.resolutions;
        var screenSet = new HashSet<(int, int)>();
        foreach (var r in screenResolutions)
            screenSet.Add((r.width, r.height));

        // 공통 해상도 중 모니터가 지원하는 것만 필터링
        foreach (var r in commonResolutions)
            if (screenSet.Contains(r))
                supportedResolutions.Add(r);

        // 하나도 없으면 현재 해상도라도 추가
        if (supportedResolutions.Count == 0)
            supportedResolutions.Add((Screen.currentResolution.width, Screen.currentResolution.height));

        resolutionDropdown.ClearOptions();
        var options = new List<string>();
        int currentIndex = supportedResolutions.Count - 1;

        for (int i = 0; i < supportedResolutions.Count; i++)
        {
            var (w, h) = supportedResolutions[i];
            options.Add($"{w} x {h}");
            if (w == Screen.currentResolution.width && h == Screen.currentResolution.height)
                currentIndex = i;
        }

        resolutionDropdown.AddOptions(options);

        int savedW = GameManager.Instance.settings.current.resolutionWidth;
        int savedH = GameManager.Instance.settings.current.resolutionHeight;
        int savedIndex = supportedResolutions.FindIndex(r => r.w == savedW && r.h == savedH);
        resolutionDropdown.value = savedIndex >= 0 ? savedIndex : currentIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    void InitWindowMode()
    {
        windowModeDropdown.ClearOptions();
        windowModeDropdown.AddOptions(new List<string> { "전체화면", "보더리스", "창모드" });
        windowModeDropdown.value = GameManager.Instance.settings.current.windowMode;
        windowModeDropdown.onValueChanged.AddListener(SetWindowMode);
    }

    void InitQuality()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string> { "낮음", "중간", "높음" });
        qualityDropdown.value = GameManager.Instance.settings.current.qualityLevel;
        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    public void SetResolution(int index)
    {
        var (w, h) = supportedResolutions[index];
        GameManager.Instance.settings.current.resolutionWidth  = w;
        GameManager.Instance.settings.current.resolutionHeight = h;
        GameManager.Instance.settings.ApplySettings();
        GameManager.Instance.settings.SaveSettings();
    }

    public void SetWindowMode(int index)
    {
        GameManager.Instance.settings.current.windowMode = index;
        GameManager.Instance.settings.ApplySettings();
        GameManager.Instance.settings.SaveSettings();
    }

    public void SetQuality(int index)
    {
        GameManager.Instance.settings.current.qualityLevel = index;
        GameManager.Instance.settings.ApplySettings();
        GameManager.Instance.settings.SaveSettings();
    }

    // ===================== 오디오 =====================

    void InitAudio()
    {
        var s = GameManager.Instance.settings.current;
        masterSlider.value  = s.masterVolume;
        bgmSlider.value     = s.bgmVolume;
        sfxSlider.value     = s.sfxVolume;
        ambientSlider.value = s.ambientVolume;

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        ambientSlider.onValueChanged.AddListener(SetAmbientVolume);
    }

    public void SetMasterVolume(float value)
    {
        GameManager.Instance.settings.current.masterVolume = value;
        GameManager.Instance.settings.ApplySettings();
        GameManager.Instance.settings.SaveSettings();
    }

    public void SetBGMVolume(float value)
    {
        GameManager.Instance.settings.current.bgmVolume = value;
        GameManager.Instance.settings.ApplySettings();
        GameManager.Instance.settings.SaveSettings();
    }

    public void SetSFXVolume(float value)
    {
        GameManager.Instance.settings.current.sfxVolume = value;
        GameManager.Instance.settings.ApplySettings();
        GameManager.Instance.settings.SaveSettings();
    }

    public void SetAmbientVolume(float value)
    {
        GameManager.Instance.settings.current.ambientVolume = value;
        GameManager.Instance.settings.ApplySettings();
        GameManager.Instance.settings.SaveSettings();
    }
}
