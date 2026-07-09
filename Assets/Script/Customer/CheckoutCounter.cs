using System.Collections.Generic;
using UnityEngine;

public class CheckoutCounter : MonoBehaviour, IInteractable
{
    public List<Transform> queuePoints;
    public Transform exitPoint;

    Queue<CustomerAI> customerQueue = new Queue<CustomerAI>();

    CustomerAI currentCustomer;

    public bool CanEnterQueue()
    {
        return customerQueue.Count < queuePoints.Count;
    }

    public void Enqueue(CustomerAI customer)
    {
        customerQueue.Enqueue(customer);
        UpdateQueuePositions();
    }

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

    public void RemoveFromQueue(CustomerAI customer)
    {
        Queue<CustomerAI> newQueue = new Queue<CustomerAI>();

        foreach (var c in customerQueue)
        {
            if (c != customer)
                newQueue.Enqueue(c);
        }

        customerQueue = newQueue;

        UpdateQueuePositions();
    }

    public bool IsFirst(CustomerAI customer)
    {
        if (customerQueue.Count == 0) return false;
        return customerQueue.Peek() == customer;
    }

    public void Interact()
    {
        if (currentCustomer == null) return;

        if (!currentCustomer.isAtCounter)
        {
            return;
        }

        ProcessCheckout(currentCustomer);
    }

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

        UpdateQueuePositions();
    }

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