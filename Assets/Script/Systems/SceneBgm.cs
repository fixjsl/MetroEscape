using UnityEngine;

public class SceneBgm : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private AudioClip bgm;
    void Start() => GameManager.Instance.audio.PlayBGM(bgm);
}
