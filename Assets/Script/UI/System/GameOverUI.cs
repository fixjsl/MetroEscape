using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; 
using System.Collections.Generic;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private TMP_Text unlockLabel;
    void Awake()
    {
        panel.SetActive(false);
        mainMenuButton.onClick.AddListener(GoMainMenu);
    }

    void Start()
    {
        GameManager.Instance.OnGameOver += Show;
    }

    private void Show()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var newly = MetaProgressManager.Instance?.EvaluateUnlocks();
        if (unlockLabel != null)
        {
            if (newly != null && newly.Count > 0)
            {
                var names = new List<string>();
                foreach (var p in newly) names.Add(p.PerkName);
                unlockLabel.text = "새 특성 해금: " + string.Join(", ", names);
            }
            else unlockLabel.text = "";
        }
    }

    private void GoMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= Show;
    }
}
