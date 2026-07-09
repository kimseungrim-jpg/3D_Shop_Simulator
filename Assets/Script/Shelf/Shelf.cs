using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Shelf : MonoBehaviour, IInteractable
{
    public ShelfSlot[] slots;

    PlayerInventory player;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerInventory>();
    }
    
    public bool HasStock()
    {
        foreach (var slot in slots)
        {
            if(!slot.IsEmpty())
                return true;
        }
        return false;
    }

    public List<ItemData> TakeItems(int amount)
    {
        List<ItemData> items = new List<ItemData>();

        foreach (var slot in slots)
        {
            if (!slot.IsEmpty())
            {
                items.Add(slot.TakeItem());

                if (items.Count >= amount)
                    break;
            }
        }

        return items;
    }

    public string GetInteractText()
    {
        return "E - Áø¿­ÇÏ±â";
    }

    public void Interact()
    {
        if (player.currentItemData == null)
            return;

        foreach (var slot in slots)
        {
           if (slot.IsEmpty())
            {
                if (slot.PlaceItem(player.currentItem))
                {
                    player.currentItem = null;
                    player.currentItemData = null;
                    return;
                }
            }
        }

        Debug.Log("ºó ½½·ÔÀÌ ¾øÀ½!");
    }
}
