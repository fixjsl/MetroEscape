using System;
using System.Collections;
using System.IO;
using UnityEngine;

using UnityEngine.Audio;

[Serializable]
public class GameSettings
{
    public int resolutionWidth  = -1;
    public int resolutionHeight = -1;
    public int windowMode       = 0;    // 0=전체화면, 1=보더리스, 2=창모드
    public int qualityLevel     = 2;    // 0=낮음, 1=중간, 2=높음
    public float masterVolume   = 1f;
    public float bgmVolume      = 1f;
    public float sfxVolume      = 1f;
    public float ambientVolume  = 1f;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    public GameSettings settings { get; private set; } = new GameSettings();

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip defaultBGM;

    public int Day{get; private set;} = 1;  //진행 일
    public int Hour{get; private set;}  = 0; // 시간
    public int Minute{get; private set;} = 0; //분

    public event Action<int, int> ChangeTime; // hour, minute
    public event Action<int, int> ChangeDay; // hour, day
    public event Action OnGameClear;
    public event Action OnGameOver;
    private string savePath => Path.Combine(Application.persistentDataPath, "settings.json");

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadSettings();
        ApplySettings();
        PlayBGM(defaultBGM);
    }

    public void SaveSettings()
    {
        File.WriteAllText(savePath, JsonUtility.ToJson(settings, true));
    }

    void LoadSettings()
    {
        if (File.Exists(savePath))
            JsonUtility.FromJsonOverwrite(File.ReadAllText(savePath), settings);
        // 파일 없으면 GameSettings 기본값 그대로 사용
    }

    public void ApplySettings()
    {
        // 해상도 & 창모드
        int w = settings.resolutionWidth  > 0 ? settings.resolutionWidth  : Screen.currentResolution.width;
        int h = settings.resolutionHeight > 0 ? settings.resolutionHeight : Screen.currentResolution.height;

        FullScreenMode mode = settings.windowMode switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            _ => FullScreenMode.Windowed
        };
        Screen.SetResolution(w, h, mode);

        // 품질
        QualitySettings.SetQualityLevel(settings.qualityLevel);

        // 오디오
        SetMixerVolume("Master",  settings.masterVolume);
        SetMixerVolume("BGM",     settings.bgmVolume);
        SetMixerVolume("SE",     settings.sfxVolume);
        SetMixerVolume("Ambient", settings.ambientVolume);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource?.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayAmbient(AudioClip clip)
    {
        if (ambientSource == null || clip == null) return;
        ambientSource.clip = clip;
        ambientSource.Play();
    }

    public void StopAmbient()
    {
        ambientSource?.Stop();
    }

    void SetMixerVolume(string param, float value)
    {
        audioMixer.SetFloat(param, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
    }

    public void GameClear()
    {
        OnGameClear?.Invoke();
    }

    public void GameOver()
    {
        OnGameOver?.Invoke();
    }

    private Coroutine clockCoroutine;

    public void TutorialStart()
    {
        if (clockCoroutine != null) StopCoroutine(clockCoroutine);
        Day = 1; Hour = 0; Minute = 0;
        clockCoroutine = StartCoroutine(Clock());
    }

    IEnumerator Clock()
    {
        while (true)
        {
            yield return YeildCache.GetIntervals(2.5f);
            Minute += 1;
            if (Minute >= 60)
            {
                Minute = 0;
                Hour += 1;
            }
            if (Hour >= 24)
            {
                Hour = 0;
                Day += 1;
                ChangeDay?.Invoke(Day, Hour);
            }
            ChangeTime?.Invoke(Hour, Minute);

        }
        
    }
}
