using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON 파일에 저장될 전체 게임 진행 데이터
/// Unity 오브젝 참조를 직접 저장하지 않고, 날짜, 돈, 재고, 진열대 상태처럼 복원 가능한 순수 데이터만 저장
/// </summary>

[Serializable]
public class SaveData
{
    public int saveVersion = 1;

    public int day;
    public int money;

    public int totalSales;
    public int dailySales;

    public int purchaseCost;
    public int maintenanceCost;

    public string saveAt;

    public HeldItemSaveData helfItem = new HeldItemSaveData();

    public List<StockItemSaveData> stockItems = new List<StockItemSaveData>();
    public List<ShelfSlotSaveData> shelfSlots = new List<ShelfSlotSaveData>();
    public List<ReturnZoneItemSaveData> returnZoneItems = new List<ReturnZoneItemSaveData>();
}

/// <summary>
/// 진열하지 못하고 보관 중인 재고 아이템 데이터를 저장합니다.
/// ItemData 자체가 아니라 itemId를 저장해야 로드 시 다시 ItemData를 찾아 복원
/// </summary>
[Serializable]
public class StockItemSaveData
{
    public string itemid;
    public int amount = 1;

    public Vector3 position;
    public Vector3 eulerAngles;

    public StockItemSaveData(string itemid, Vector3 position, Vector3 eulerAngles)
    {
        this.itemid = itemid;
        this.position = position;
        this.eulerAngles = eulerAngles;
        amount = 1;
    }

    // 더미 데이터 테스트를 위한 함수
    public StockItemSaveData(string itemid, int amount)
    {
        this.itemid = itemid;
        this.amount = amount;
        position = Vector3.zero;
        eulerAngles = Vector3.zero;
    }
}

/// <summary>
/// 진열대의 특정 슬롯에 어떤 아이템이 몇 개 올라가 있는지 저장
/// shelfId와 slotIndex를 이용해 로드 시 정확한 진열 위치를 복원
/// </summary>
[Serializable]
public class ShelfSlotSaveData
{
    public string shelfId;
    public int slotIndex;

    public string itemId;
    public int amount;

    public ShelfSlotSaveData(string shelfId, int slotIndex, string itemId, int amount)
    {
        this.shelfId = shelfId;
        this.slotIndex = slotIndex;
        this.itemId = itemId;
        this.amount = amount;
    }
}

/// <summary>
/// 플레이어가 손에 들고 있는 아이템을 저장하기 위한 데이터
/// 아이템 오브젝트 자체를 저장하지 않고 ItemID만 저장
/// 로드 시 ItemDatabase를 통해 다시 ItemData를 찾아 복원
/// </summary>
[Serializable]
public class HeldItemSaveData
{
    public bool hasItem;
    public string itemId;

    public HeldItemSaveData()
    {
        hasItem = false;
        itemId = string.Empty;
    }

    public HeldItemSaveData(bool hasItem, string itemId)
    {
        this.hasItem = hasItem;
        this.itemId = itemId;
    }
}

/// <summary>
/// 반납함에 보관 중인 아이템을 저장하기 위한 데이터
/// ItemData 자체를 저장하지 않고 ItemId만 저장
/// 로드 시 ItemDatabase를 통해 다시 ItemData를 찾아 복원
/// </summary>
[Serializable]
public class ReturnZoneItemSaveData
{
    public string ItemId;
    public int amount;

    public ReturnZoneItemSaveData(string itemId, int amount)
    {
        this.ItemId = itemId;
        this.amount = amount;
    }
}