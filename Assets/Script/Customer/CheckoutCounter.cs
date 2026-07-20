using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 계산대의 손님 대기열을 관리
/// 플레이어와 상호작용할 때 맨 앞 손님의 상품 금액을 계산하여 결재를 처리
/// </summary>
public class CheckoutCounter : MonoBehaviour, IInteractable
{
    public List<Transform> queuePoints;
    public Transform exitPoint;

    Queue<CustomerAI> customerQueue = new Queue<CustomerAI>();

    CustomerAI currentCustomer;

    /// <summary>
    /// 계산대 대기열에 새로운 손님이 들어갈 자리가 있는지 체크
    /// 손님이 계산대로 이동하기 전에 호출
    /// </summary>
    public bool CanEnterQueue()
    {
        return customerQueue.Count < queuePoints.Count;
    }

    /// <summary>
    /// 손님을 마지막 대기열에 추가하고 모든 손님의 위치를 갱신시킴
    /// 손님이 계산대 대기열에 참여하기로 했을 때 호출
    /// </summary>
    public void Enqueue(CustomerAI customer)
    {
        customerQueue.Enqueue(customer);
        UpdateQueuePositions();
    }

    /// <summary>
    /// 현재 대기 순서에 맞춰 각 손님의 목적지를 배치
    /// 결제 대상이 되는 맨 앞 손님을 갱신
    /// 대기열에 손님이 추가되거나 제거된 직후 호출
    /// </summary>
    void UpdateQueuePositions()
    {
        int index = 0;

        foreach (var customer in customerQueue)
        {
            if (index < queuePoints.Count)
            {
                customer.SetQueuePosition(queuePoints[index]);
            }
            index++;
        }

        currentCustomer = customerQueue.Count > 0 ? customerQueue.Peek() : null;
    }

    /// <summary>
    /// 지정한 손님만 대기열에서 제외한 뒤 남은 손님의 위치를 다시 정렬
    /// 손님이 결제 전에 대기열을 이탈하는 상황에서 호출
    /// </summary>
    public void RemoveFromQueue(CustomerAI customer)
    {
        Queue<CustomerAI> newQueue = new Queue<CustomerAI>();

        //Queue는 중간 요소를 직접 제거할 수 없어 새로운 큐에서
        //제거 대상을 제외한 손님들로 새 대기열을 구성해 기존 순서를 유지
        foreach (var c in customerQueue)
        {
            if (c != customer)
                newQueue.Enqueue(c);
        }

        customerQueue = newQueue;

        UpdateQueuePositions();
    }

    /// <summary>
    /// 지정한 손님이 현재 결제 순서의 첫 번쨰 손님인지 확인
    /// 손님 AI가 계산대로 이동하거나 결재를 기다릴 수 있는지 판단할 때 호출
    /// </summary>
    public bool IsFirst(CustomerAI customer)
    {
        if (customerQueue.Count == 0) return false;
        return customerQueue.Peek() == customer;
    }

    /// <summary>
    /// 플레이어가 계산대와 상호작용했을 때 맨 앞 손님의 결재를 시도
    /// IInteractable을 감지한 플레이어 상호작용 시스템에서 호출
    /// </summary>
    public void Interact()
    {
        if (currentCustomer == null) return;

        //맨 앞 순서라도 손님이 계산대 위치에 도착하기 전에는 결제를 하지 않음
        if (!currentCustomer.isAtCounter)
        {
            return;
        }

        ProcessCheckout(currentCustomer);
    }

    /// <summary>
    /// 손님 카트의 총액을 정산하고 결제를 마친 손님을 대기열에서 내보냄
    /// 유효한 손님이 계산대에 도착한 상태에서 Interact를 통해 호출됨
    /// </summary>
    void ProcessCheckout(CustomerAI customer)
    {
        int totalPrice = 0;

        foreach (var item in customer.cart)
        {
            totalPrice += item.sellPrice;
        }

        GameManager.instance.AddMoney(totalPrice);

        customerQueue.Dequeue();

        customer.FinishCheckout();

        //결제 완료 후 다음 손님을 앞으로 이동시키고 새로운 결제 대상을 지정
        UpdateQueuePositions();
    }

    /// <summary>
    /// 현재 결제 가능한 손님의 상품 목록과 가격을 상호작용 안내 문구로 반환
    /// 플레이어 상호작용 UI가 표시 문구를 갱신할 때 호출
    /// </summary>
    public string GetInteractText()
    {
        if(currentCustomer == null) return "";

        if (!currentCustomer.isAtCounter) return "";

        string text = "E - 계산하기\n";

        foreach (var item in currentCustomer.cart)
        {
            text += item.itemName + " : " + item.sellPrice + "원\n";
        }

        return text;
    }
}