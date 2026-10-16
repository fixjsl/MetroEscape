using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameClearUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    void Awake()
    {
        panel.SetActive(false);
        mainMenuButton.onClick.AddListener(GoMainMenu);
    }

    void Start()
    {
        GameManager.Instance.OnGameClear += Show;
    }

    private void Show()
    {
        Debug.Log("되냐");
        panel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void GoMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameClear -= Show;
    }
}
