using UnityEngine;

public class ShelfSlot : MonoBehaviour
{
    public Item currentItem;

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public bool PlaceItem(Item item)
    {
        if (item == null || !IsEmpty())
            return false;

        currentItem = item;

        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        return true;
    }

    public ItemData TakeItem()
    {
        if(IsEmpty()) return null;

        Item item = currentItem;
        currentItem = null;

        ItemData data = item.data;

        Destroy(item.gameObject);

        return data;
    }
}
