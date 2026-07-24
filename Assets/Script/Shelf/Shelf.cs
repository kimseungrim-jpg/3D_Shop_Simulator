using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 여러 진열대 슬롯의 재고 상태를 관리
/// 플레이어의 상품 진열과 손님이 요청한 수량만큼 상품을 가져가는 처리를 연결
/// Save/Load 시에는 각 슬롯에 어떤 아이템이 올라가 있는지 저장하고 복원
/// </summary>
public class Shelf : MonoBehaviour, IInteractable
{
    [Header("저장/로드 ID")]
    [SerializeField] private string shelfID;

    [Header("진열 슬롯")]
    public ShelfSlot[] slots;

    PlayerInventory player;

    public string ShelfID => shelfID;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerInventory>();

        if (string.IsNullOrWhiteSpace(shelfID))
        {
            Debug.LogWarning($"[Shelf] shelfID가 비어 있습니다: {gameObject.name}");
        }
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
    /// 진열대에서 지정한 수량만큼 아이템을 꺼냄
    /// 손님이 상품을 고를 때 호출
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

    /// <summary>
    /// 현재 진열대 슬롯 상태를 저장 데이터로 변환
    /// GameManager가 전체 저장 데이터를 만들 때 호출
    /// </summary>
    public List<ShelfSlotSaveData> CreateShelfSlotSaveData()
    {
        List<ShelfSlotSaveData> saveDataList = new List<ShelfSlotSaveData>();

        if (string.IsNullOrWhiteSpace(shelfID))
        {
            Debug.LogWarning($"[Shelf] shelfID가 없어 저장에서 제외됩니다.");
            return saveDataList;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            ShelfSlot slot = slots[i];

            if (slot == null || slot.IsEmpty())
            {
                continue;
            }

            ItemData itemData = slot.GetCurrentItemData();

            if (itemData == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(itemData.itemID))
            {
                Debug.LogWarning($"[Shelf] itemID가 없는 아이템은 저장할 수 없습니다: {itemData.itemName}");
                continue;
            }

            saveDataList.Add(new ShelfSlotSaveData(shelfID, i, itemData.itemID, 1));
        }

        return saveDataList;
    }

    /// <summary>
    /// 진열대의 모든 슬롯을 비움
    /// 저장 데이터를 적용하기 전에 기존 아이템이 중복으로 남지 않도록 함
    /// </summary>
    public void ClearAllSlots()
    {
        foreach (ShelfSlot slot in slots)
        {
            if (slot == null)
            {
                continue;
            }

            slot.ClearSlot();
        }
    }

    /// <summary>
    /// 저장 데이터 목록 중 이 진열대에 해당하는 데이터만 찾아 슬롯에 복원
    /// 이어하기로 ShopScene에 진입했을 때 GameManager가 호출
    /// </summary>
    public void ApplyLoadedShelfData(List<ShelfSlotSaveData> shelfSlotSaveDataList)
    {
        if (shelfSlotSaveDataList == null)
        {
            return;
        }

        ClearAllSlots();

        foreach (ShelfSlotSaveData slotData in shelfSlotSaveDataList)
        {
            if (slotData.shelfId != shelfID)
            {
                continue;
            }

            if (slotData.slotIndex < 0 || slotData.slotIndex >= slots.Length)
            {
                Debug.LogWarning($"[Shelf] 잘못된 슬롯 인덱스입니다. Shelf: {shelfID}, slot: {slotData.slotIndex}");
                continue;
            }

            if (!ItemDatabase.Instance.TryGetItemData(slotData.itemId, out ItemData itemData))
            {
                continue;
            }

            slots[slotData.slotIndex].RestoreItem(itemData);
        }
    }
}
