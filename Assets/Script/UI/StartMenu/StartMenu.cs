using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private GameObject StartMenuPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button     BackButton;

    [Header("Intro Video")]
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Button skipButton;

    void Awake()
    {
        startButton.onClick.AddListener(OnStart);
        settingsButton.onClick.AddListener(OnSettings);
        quitButton.onClick.AddListener(OnQuit);
        BackButton.onClick.AddListener(OnExitSetting);

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(LoadGameScene);
            skipButton.gameObject.SetActive(false);
        }

        if (videoPlayer != null)
            videoPlayer.loopPointReached += _ => LoadGameScene();
    }

    void OnStart()
    {
        if (PerkSelectUI.Instance == null) { OnPerksSelected(null); return; }

        StartMenuPanel.SetActive(false);
        PerkSelectUI.Instance.Show(OnPerksSelected, OnPerkSelectCancelled);
    }

    private void OnPerksSelected(IReadOnlyList<PerkBase> perks)
    {
        MetaProgressManager.Instance?.SetPending(perks);
        PlayIntroThenLoad();
    }

    // 퍽 선택을 취소하면 게임을 시작하지 않고 시작 메뉴로 되돌린다.
    private void OnPerkSelectCancelled()
    {
        StartMenuPanel.SetActive(true);
    }
    private void PlayIntroThenLoad()
{
    if (videoPlayer == null || videoPlayer.clip == null)
    {
        LoadGameScene();
        return;
    }

    StartMenuPanel.SetActive(false);
    videoPanel.SetActive(true);
    skipButton.gameObject.SetActive(true);
    videoPlayer.Play();
}

    void LoadGameScene()
    {
        SceneFlow.Go("Base&Main1");   // Core 위에 게임 씬을 올림 (Core는 그대로)
    }

    void OnSettings()
    {
        StartMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    void OnExitSetting()
    {
        StartMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
