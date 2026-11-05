using System;
using System.Collections;
using UnityEngine;

using UnityEngine.Audio;

[Serializable]
public class GameSettings
{
    public int resolutionWidth = -1;
    public int resolutionHeight = -1;
    public int windowMode = 0;    // 0=전체화면, 1=보더리스, 2=창모드
    public int qualityLevel = 2;    // 0=낮음, 1=중간, 2=높음
    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public float ambientVolume = 1f;
}

public class GameManager : SingletonBehaviour<GameManager>
{

    public GameSetting settings { get; private set; }

    public GameClock clock { get; private set; }
    public GameAudio audio { get; private set; }

    protected override bool Persistent => true;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;




    public event Action OnGameClear;
    public event Action OnGameOver;


    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        audio = new GameAudio(audioMixer,bgmSource,sfxSource,ambientSource);
        clock = new GameClock();
        settings = new GameSetting(audio);
    }

    public void GameClear()
    {
        OnGameClear?.Invoke();
    }

    public void GameOver()
    {
        MetaProgressManager.Instance?.OnGameOver();
        OnGameOver?.Invoke();
    }


    private Coroutine clockRoutine;
    public void ClockStart()
    {
        clock.Restart();
        if (clockRoutine != null) StopCoroutine(clockRoutine);
        clockRoutine = StartCoroutine(ClockRoutine());
    }
    IEnumerator ClockRoutine()
    {
        var wait = YeildCache.GetIntervals(2.5f);
        while (true) { yield return wait; clock.Tick(); }
    }

}
