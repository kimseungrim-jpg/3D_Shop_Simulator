using UnityEngine;

/// <summary>
/// 계산대 Trigger에 진입한 손님의 카트 금액을 계산하여 소지금에 반영
/// 계산을 마친 손님 오브젝트를 제거하는 자동 계산대 처리용 스크립트
/// </summary>
public class Counter : MonoBehaviour
{
    public int itemPrice = 100;

    /// <summary>
    /// Trigger에 들어온 오브젝트가 손님인지 확인하고 해당 손님의 결제를 처리
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Customer"))
        {
            CustomerAI customer = other.GetComponent<CustomerAI>();

            if (customer != null)
            {
                int total = 0;

                foreach (var item in customer.cart)
                {
                   // total += item.
                }

                GameManager.instance.AddMoney(total);

                Destroy(customer.gameObject);
            }
        }
    }
}
