using NUnit.Framework;
using System;
using System.Collections.Generic;

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

    public List<StockItemSaveData> stockItems = new List<StockItemSaveData>();
    public List<ShelfSlotSaveData> shelfSlots = new List<ShelfSlotSaveData>();
}

/// <summary>
/// 진열하지 못하고 보관 중인 재고 아이템 데이터를 저장합니다.
/// ItemData 자체가 아니라 itemId를 저장해야 로드 시 다시 ItemData를 찾아 복원
/// </summary>
[Serializable]
public class StockItemSaveData
{
    public string itemid;
    public int amount;

    public StockItemSaveData(string itemid, int amount)
    {
        this.itemid = itemid;
        this.amount = amount;
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