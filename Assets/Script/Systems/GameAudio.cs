using UnityEngine;
using UnityEngine.Audio;

public class GameAudio
{
    public AudioMixer audioMixer { get; private set; }
    public AudioSource bgmSource { get; private set; }
    public AudioSource sfxSource { get; private set; }
    public AudioSource ambientSource { get; private set; }

    public GameAudio(AudioMixer audioMixer,AudioSource bgmSource,AudioSource sfxSource,AudioSource ambientSource)
    {
        this.audioMixer = audioMixer;
        this.bgmSource = bgmSource;
        this.sfxSource = sfxSource;
        this.ambientSource = ambientSource;
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


}
