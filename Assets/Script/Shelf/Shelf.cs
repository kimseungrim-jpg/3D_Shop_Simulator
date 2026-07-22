using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 여러 진열대 슬롯의 재고 상태를 관리
/// 플레이어의 상품 진열과 손님이 요청한 수량만큼 상품을 가져가는 처리를 연결
/// </summary>
public class Shelf : MonoBehaviour, IInteractable
{
    public ShelfSlot[] slots;

    PlayerInventory player;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerInventory>();
    }
    
    /// <summary>
    /// 진열대 슬롯 중 하나라도 상품을 보유하고 있는지 확인
    /// CustomerAI가 방문할 진열대를 선택하거나 이동 중 재고를 다시 확인할 때 호출
    /// </summary>
    public bool HasStock()
    {
        foreach (var slot in slots)
        {
            if(!slot.IsEmpty())
                return true;
        }
        return false;
    }

    /// <summary>
    /// 앞쪽 슬롯부터 상품을 하나씩 꺼내 요청 수량만큼의 아이템 데이터를 반환
    /// </summary>
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
        return "E - 진열하기";
    }

    /// <summary>
    /// 플레이어가 들고 있는 상품을 첫 번째 빈 슬롯에 배치하고 보유 참조를 비움
    /// </summary>
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

        Debug.Log("빈 슬롯이 없음!");
    }
}
