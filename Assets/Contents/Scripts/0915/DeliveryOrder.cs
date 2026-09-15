using UnityEngine;
using UnityEngine.Events;

//간단 한 배달 주문
[System.Serializable]
public class DeliveryOrder 
{
    public int orderId;
    public string restaurantName;
    public string customerName;
    public Building restaurantBuilding;
    public Building customerBuilding;

    public float orderTime;
    public float timeLimit;
    public float reward;
    public OrderState state;

    //생성자
    public DeliveryOrder(int id, Building restaurnat, Building customer, float rewardAmount)
    {
        orderId = id;
        restaurantBuilding = restaurnat;
        customerBuilding = customer;
        restaurantName = restaurnat.buildingName;
        customerName = customer.buildingName;
        orderTime = Time.time;
        timeLimit = Random.Range(60f, 120f);                //1 ~ 2분 제한
        reward = rewardAmount;
        state = OrderState.WaitingPickup;
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0f, timeLimit - (Time.time - orderTime)); //남은 시간 리턴
    }

    public bool IsExpired()
    {
        return GetRemainingTime() <= 0f;
    }
}