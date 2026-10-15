using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private Image hpFill;
    [SerializeField] private TMP_Text hpText;

    [Header("Stamina")]
    [SerializeField] private Image staminaFillLeft;
    [SerializeField] private Image staminaFillRight;

    [Header("Hungry")]
    [SerializeField] private Image hungryFill;

    [Header("Water")]
    [SerializeField] private Image waterFill;

    [SerializeField] private TMP_Text HourMinute;
    [SerializeField] private TMP_Text Day;


    private PlayerStatus status;

    void Start()
    {
        status = PlayerStateMachine.Instance.status;

        status.ChangeHP       += OnHPChanged;
        status.ChangeStamina  += OnStaminaChanged;
        status.ChangeHungry   += OnHungryChanged;
        status.ChangeWater    += OnWaterChanged;
        GameManager.Instance.ChangeTime += OnChangeTime;
        GameManager.Instance.ChangeDay += OnChangeDay;

        // 초기값 반영
        SetFill(hpFill,      status.Hp,      status.Maxhp);
        hpText.text = $"{(int)status.Hp} / {(int)status.Maxhp}";
        SetStaminaFill(status.Stamina, status.MaxStamina);
        SetFill(hungryFill,  status.Hungry,   status.MaxHungry);
        SetFill(waterFill,   status.Water,    status.MaxWater);
        HourMinute.text = $"{GameManager.Instance.Hour:D2}:{GameManager.Instance.Minute:D2}";
        Day.text = $"Day - {GameManager.Instance.Day}";
    }

    void OnHPChanged(float val)
    {
        SetFill(hpFill,      val, status.Maxhp);
        hpText.text = $"{(int)val} / {(int)status.Maxhp}";
    }  
        
    
    void OnStaminaChanged(float val) => SetStaminaFill(val, status.MaxStamina);
    void OnHungryChanged(int val)    => SetFill(hungryFill,  val, status.MaxHungry);
    void OnWaterChanged(int val)     => SetFill(waterFill,   val, status.MaxWater);

    void SetStaminaFill(float current, float max)
    {
        float ratio = current / max;
        if (staminaFillLeft)  staminaFillLeft.fillAmount  = ratio;
        if (staminaFillRight) staminaFillRight.fillAmount = ratio;
    }

    void SetFill(Image image, float current, float max)
    {
        if (image == null) return;
        image.fillAmount = current / max;
    }

    void OnDestroy()
    {
        if (status == null) return;
        if (GameManager.Instance == null) return;
        status.ChangeHP      -= OnHPChanged;
        status.ChangeStamina -= OnStaminaChanged;
        status.ChangeHungry  -= OnHungryChanged;
        status.ChangeWater   -= OnWaterChanged;
        GameManager.Instance.ChangeTime -= OnChangeTime;
        GameManager.Instance.ChangeDay -= OnChangeDay;
    }

    void OnChangeTime(int hour, int minute)
    {
        HourMinute.text = $"{hour:D2}:{minute:D2}";
    }
    void OnChangeDay(int day, int hour)
    {
        Day.text = $"Day - {day}";
    }
}
