using UnityEngine;
using UnityEngine.UI;

public class GameClearUI : UIPanel
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    void Awake()
    {
        root.SetActive(false);
        mainMenuButton.onClick.AddListener(GoMainMenu);
    }

    void Start()
    {
        GameManager.Instance.OnGameClear += Show;
    }

    private void Show() => Open();

    protected override void OnOpened()
    {
        Debug.Log("되냐");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void GoMainMenu()
    {
        Time.timeScale = 1f;
        UIStackManager.Clear();
        SceneFlow.Go(mainMenuSceneName);   // Core 빼고 내리고 메뉴 씬을 올림
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameClear -= Show;
    }
}
