
using System.Collections.Generic;
using UnityEngine;

public class ReturnZone : MonoBehaviour, IInteractable
{
    public List<ItemData> storedItems = new List<ItemData>();

    public void AddItems(List<ItemData> items)
    {
        storedItems.AddRange(items);
    }

    public ItemData TakeItem()
    {
        if(storedItems.Count == 0) return null;

        ItemData item = storedItems[0];
        storedItems.RemoveAt(0);
        return item;
    }

    public string GetInteractText()
    {
        return storedItems.Count > 0 ? "E - 아이템 가져가기" : "";
    }

    public void Interact()
    {
        // Player에서 처리 (비워둬도 OK)
    }
}