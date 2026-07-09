using System.Runtime.Serialization;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Transform hand;

    public Item currentItem;
    public ItemData currentItemData;
    public GameObject holdItem;

    public Transform holdPoint;

    public int currentItems = 0;
    public int maxItems = 10;

    public void AddItems(int amount)
    {
        currentItems = Mathf.Min(currentItems + amount, maxItems);
    }

    public void SetItem(ItemData data)
    {      
        if (holdItem != null)
        {
            Destroy(holdItem);
        }

        GameObject obj = Instantiate(data.prefabs, holdPoint);

        Item item = obj.GetComponent<Item>();

        Rigidbody rb = item.GetComponent<Rigidbody>();

        rb.isKinematic = true;
        currentItem = item;
        currentItemData = data;
    }

    public void ClearItem()
    {
        currentItemData = null;
        currentItem = null;

        if (holdItem != null)
        {
            Destroy(holdItem);
        }
    }

    public int RemoveItems(int amount)
    {
        int remove = Mathf.Min(amount, currentItems);
        currentItems -= remove;
        return remove;
    }

    public void PickUpItem(Item item)
    {
        if (currentItem != null)
        {
            return;
        }

        currentItem = item;
        currentItemData = item.data;   

        Rigidbody rb = item.GetComponent<Rigidbody>();
        Collider col = item.GetComponent<Collider>();

        if (rb != null)
        {
            rb.isKinematic = true;
            //rb.linearVelocity = Vector3.zero;
            //rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        item.transform.SetParent(hand);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }
}
