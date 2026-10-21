using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class PauseMenuUI : UIPanel
{
    public static PauseMenuUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private TMP_Text hoverDescText;


    void Awake()
    {
        Instance = this;
        root.SetActive(false);

        resumeButton.onClick.AddListener(Resume);
        mainMenuButton.onClick.AddListener(GoMainMenu);
        AddHoverDesc(resumeButton, "계속하기");
        AddHoverDesc(mainMenuButton, "타이틀로 돌아가기");
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!UIStackManager.HandleCancel()) Open();
        }
    }

    public void Resume() => Close();

    private void GoMainMenu()
    {
        Time.timeScale = 1f;
        UIStackManager.Clear();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void AddHoverDesc(Button btn, string desc)
    {
        var trigger = btn.gameObject.GetComponent<EventTrigger>() ?? btn.gameObject.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => { if (hoverDescText != null) hoverDescText.text = desc; });
        trigger.triggers.Add(enter);
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => { if (hoverDescText != null) hoverDescText.text = ""; });
        trigger.triggers.Add(exit);
    }

}
