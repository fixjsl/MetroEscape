using System;

// 예/아니오 확인창 (ConfirmUI가 구현)
public interface IConfirmDialog
{
    void Show(string message, Action onConfirm, Action onCancel = null);
}
