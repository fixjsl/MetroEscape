using System.IO;
using UnityEngine;
public class GameSetting
{
    public GameSettings current { get; private set; }
    private string savePath => Path.Combine(Application.persistentDataPath, "settings.json");
    public void SaveSettings() => JsonFileStore.Save(savePath, current);


    void LoadSettings() => current = JsonFileStore.LoadorDefault<GameSettings>(savePath);

    public GameAudio audio;

    public GameSetting(GameAudio audio)
    {
        this.audio = audio;
        LoadSettings();
        SaveSettings();
        ApplySettings();
    }

    public void ApplySettings()
    {
        // 해상도 & 창모드
        int w = current.resolutionWidth > 0 ? current.resolutionWidth : Screen.currentResolution.width;
        int h = current.resolutionHeight > 0 ? current.resolutionHeight : Screen.currentResolution.height;

        FullScreenMode mode = current.windowMode switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            _ => FullScreenMode.Windowed
        };
        Screen.SetResolution(w, h, mode);

        // 품질
        QualitySettings.SetQualityLevel(current.qualityLevel);

        // 오디오
        SetMixerVolume("Master", current.masterVolume);
        SetMixerVolume("BGM", current.bgmVolume);
        SetMixerVolume("SE", current.sfxVolume);
        SetMixerVolume("Ambient", current.ambientVolume);
    }
    void SetMixerVolume(string param, float value)
    {
        audio.audioMixer.SetFloat(param, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
    }
}
