using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private TMP_Text hoverDescText;

    private bool isPaused = false;

    void Awake()
    {
        Instance = this;
        pausePanel.SetActive(false);

        resumeButton.onClick.AddListener(Resume);
        mainMenuButton.onClick.AddListener(GoMainMenu);
        AddHoverDesc(resumeButton, "계속하기");
        AddHoverDesc(mainMenuButton, "타이틀로 돌아가기");
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (CraftingUI.Instance != null && CraftingUI.Instance.IsOpen)
                CraftingUI.Instance.Close();
            else if (ContainerUI.Instance != null && ContainerUI.Instance.IsOpen)
                ContainerUI.Instance.Close();
            else if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
                InventoryUI.Instance.Close();
            else if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    private void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetCombatInput(false);
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        SetCombatInput(true);
    }

    private void GoMainMenu()
    {
        Time.timeScale = 1f;
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

    private void SetCombatInput(bool enabled)
    {
        System.Action<InputAction> toggle = enabled ? a => a.Enable() : a => a.Disable();
        toggle(InputBindings.GetAction("Attack"));
        toggle(InputBindings.GetAction("Dodge"));
        toggle(InputBindings.GetAction("Guard"));
    }
}
