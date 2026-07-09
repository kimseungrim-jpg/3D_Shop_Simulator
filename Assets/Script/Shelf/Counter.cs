using UnityEngine;

public class Counter : MonoBehaviour
{
    public int itemPrice = 100;

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
