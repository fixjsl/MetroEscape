using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ConfirmUI : UIPanel<ConfirmUI>
{

    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action _onConfirm;
    private Action _onCancel;

    bool confirmed;
    protected override void Awake()
    {
        base.Awake();
        root.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }


    public void Show(string message, Action onConfirm, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel  = onCancel;
        messageText.text = message;
        confirmed = false;
        Open();
    }

    private void OnConfirm()
    {
        confirmed = true;
        Close();
        _onConfirm?.Invoke();
    }

    private void OnCancel() => Close();
    protected override void OnClosed()
    {
        if (!confirmed)
        {
            _onCancel?.Invoke();
        }
    }


}
