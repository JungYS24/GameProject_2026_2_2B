using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryUIManager : MonoBehaviour
{
    [Header("UI 요소")]
    public Text statusText;
    public Text messageText;
    public Slider batterSlider;
    public Image batteryFill;

    public DeliveryDriver driver;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (driver != null)
        {
            driver.driverEvents.OnMoneyChanged.AddListener(UpdateMoney);
            driver.driverEvents.OnBatteryChanged.AddListener(UpdateBattery);
            driver.driverEvents.OnDeliveryCountChanged.AddListener(UpdateDeliveryCount);
            driver.driverEvents.OnMoveStarted.AddListener(OnMoveStarted);
            driver.driverEvents.OnMoveStopped.AddListener(OnMoveStopped);
            driver.driverEvents.OnLowBattery.AddListener(OnLowBattery);
            driver.driverEvents.OnBatteryEmpty.AddListener(OnBatteryEmpty);
            driver.driverEvents.OnDeliveryCompleted.AddListener(OnDeliveryCompleted);
        }

        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (statusText != null && driver != null)
        {
            statusText.text = driver.GetStatusText();
        }
    }

    void ShowMessage(string message, Color color)                      // 메세지 표시 함수
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
            StartCoroutine(ClearMessageAfterDelay(2f));
        }
    }

    IEnumerator ClearMessageAfterDelay(float delay)                     // 일정 시간 지난 후 사라진다.
    {
        yield return new WaitForSeconds(delay);
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    void UpdateMoney(float money)
    {
        ShowMessage($"돈 : {money} 원", Color.green);
    }

    void UpdateBattery(float battery)
    {
        if (batterSlider != null)
        {
            batterSlider.value = battery / 100f;
        }

        if (batteryFill != null)
        {
            if (battery > 50f)
            {
                batteryFill.color = Color.green;
            }
            else if (battery > 20f)
            {
                batteryFill.color = Color.green;
            }
            else
            {
                batteryFill.color = Color.red;
            }
        }
    }

    void UpdateDeliveryCount(int count)
    {
        ShowMessage($"배달 완료 : {count}건", Color.blue);
    }

    void OnMoveStarted()
    {
        ShowMessage("이동 시작", Color.cyan);
    }

    void OnMoveStopped()
    {
        ShowMessage("이동 정지", Color.gray);
    }

    void OnLowBattery()
    {
        ShowMessage("배터리 부족!", Color.red);
    }

    void OnBatteryEmpty()
    {
        ShowMessage("배터리 방전!", Color.red);
    }

    void OnDeliveryCompleted()
    {
        ShowMessage("배달완료!", Color.green);
    }

    void UpdateUI()
    {
        if (driver != null)
        {
            UpdateMoney(driver.currentMoney);
            UpdateBattery(driver.batteryLevel);
            UpdateDeliveryCount(driver.deliveryCount);
        }
    }

    void OnDestroy() // 객체가 파괴 될 때 호출
    {
        if (driver != null)
        {
            // 이벤트 구독 해제 
            driver.driverEvents.OnMoneyChanged.RemoveListener(UpdateMoney);
            driver.driverEvents.OnBatteryChanged.RemoveListener(UpdateBattery);
            driver.driverEvents.OnDeliveryCountChanged.RemoveListener(UpdateDeliveryCount);
            driver.driverEvents.OnMoveStarted.RemoveListener(OnMoveStarted);
            driver.driverEvents.OnMoveStopped.RemoveListener(OnMoveStopped);
            driver.driverEvents.OnLowBattery.RemoveListener(OnLowBattery);
            driver.driverEvents.OnBatteryEmpty.RemoveListener(OnBatteryEmpty);
            driver.driverEvents.OnDeliveryCompleted.RemoveListener(OnDeliveryCompleted);
        }
    }
}