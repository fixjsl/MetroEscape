using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ConfirmUI : MonoBehaviour
{
    public static ConfirmUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action _onConfirm;
    private Action _onCancel;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }

    void Update()
    {
        if (panel.activeSelf && Keyboard.current.escapeKey.wasPressedThisFrame)
            OnCancel();
    }

    public void Show(string message, Action onConfirm, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel  = onCancel;
        messageText.text = message;
        panel.SetActive(true);
        SetCombatInput(false);
    }

    private void OnConfirm()
    {
        panel.SetActive(false);
        SetCombatInput(true);
        _onConfirm?.Invoke();
    }

    private void OnCancel()
    {
        panel.SetActive(false);
        SetCombatInput(true);
        _onCancel?.Invoke();
    }

    private void SetCombatInput(bool enabled)
    {
        Action<InputAction> toggle = enabled ? a => a.Enable() : a => a.Disable();
        toggle(InputBindings.GetAction("Attack"));
        toggle(InputBindings.GetAction("Dodge"));
        toggle(InputBindings.GetAction("Guard"));
    }
}
