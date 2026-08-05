using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 바닥에 남아있는 재고 아이템의 저장/복원을 담당
/// 진열대에 올라간 아이템과 플레이어가 들고 있는 아이템은 제외
/// 월드에 남아있는 Item 오브젝트만 SaveData로 변환하거나 다시 생성
/// </summary>
public class StockItemSaveController : MonoBehaviour
{
   public static StockItemSaveController Instance { get; private set; }

    [Header("복원된 아이템을 정리해줄 부모 오브젝트")]
    [SerializeField] private Transform restoredItemParnet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// 현재 씬에 남아있는 바닥 아이템을 저장 데이터로 변환
    /// GameManager가 SaveData를 만들 때 호출
    /// </summary>
    public List<StockItemSaveData> CreateStockItemSaveData()
    {
        List<StockItemSaveData> saveDataList = new List<StockItemSaveData>();

        Item[] items = FindObjectsByType<Item>(FindObjectsSortMode.None);
        PlayerInventory playerInventory = FindAnyObjectByType<PlayerInventory>();

        foreach (Item item in items)
        {
            if (item == null || item.data == null)
            {
                continue;
            }

            if (!IsFloorStockItem(item, playerInventory))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.data.itemID))
            {
                Debug.LogWarning($"[StockItemSaveController] itemID가 없어 저장할 수 없습니다: {item.data.name}");
                continue;
            }

            saveDataList.Add(
                new StockItemSaveData(
                    item.data.itemID,
                    item.transform.position,
                    item.transform.eulerAngles
                    )
                );
        }

        Debug.Log($"[StockItemSaveController] 바닥 재고 저장 개수: {saveDataList.Count}");
        return saveDataList;
    }

    /// <summary>
    /// 저장된 바닥 아이템 데이터를 현재 씬에 복원
    /// 이어하기로 ShopScene에 진입했을 때 GameManager가 호출
    /// </summary>
    public void ApplyLoadedStockItems(List<StockItemSaveData> stockItemSaveDataList)
    {
        ClearCurrentFloorStockItems();

        if (stockItemSaveDataList == null)
        {
            return;
        }

        foreach (StockItemSaveData stockData in stockItemSaveDataList)
        {
            if (stockData == null)
            {
                continue;
            }

            if (!ItemDatabase.Instance.TryGetItemData(stockData.itemid, out ItemData itemData))
            {
                continue;
            }

            RestoreStorckItem(itemData, stockData.position, stockData.eulerAngles);
        }

        Debug.Log($"[StockItemController] 바닥 재고 복원 완료: {stockItemSaveDataList.Count}");
    }

    /// <summary>
    /// 현재 씬에 있는 바닥 아이템을 제거
    /// 저장 데이터를 적용하기 전에 기존 아이템과 복원 아이템이 중복되지 않도록 호출
    /// </summary>
    public void ClearCurrentFloorStockItems()
    {
        Item[] items = FindObjectsByType<Item>(FindObjectsSortMode.None);
        PlayerInventory playerInventory = FindAnyObjectByType<PlayerInventory>();

        foreach (Item item in items)
        {
            if (item == null)
            {
                continue;
            }

            if (!IsFloorStockItem(item, playerInventory))
            {
                continue;
            }

            Destroy(item.gameObject);
        }
    }

    /// <summary>
    /// 저장된 ItemData와 위치 정보를 기준으로 바닥 아이템을 다시 생성
    /// 로드 시 저장된 월드 위치와 회전값을 복원하기 위해 호출
    /// </summary>
    public void RestoreStorckItem(ItemData itemData, Vector3 position, Vector3 eulerAngles)

    {
        if (itemData == null)
        {
            Debug.LogWarning("[StockItemSaveController] 복원할 itemData가 없습니다.");
            return;
        }

        if (itemData.prefabs == null)
        {
            Debug.LogWarning($"[StockItemSaveController] {itemData.itemName}의 프리팹이 없습니다.");
            return;
        }

        GameObject itemObject = Instantiate(
            itemData.prefabs,
            position,
            Quaternion.Euler(eulerAngles),
            restoredItemParnet
            );

        Item item = itemObject.GetComponent<Item>();

        if (item == null)
        {
            Debug.LogWarning($"[StockItemSaveController] 복원된 프리팹에 Item 컴포넌트가 없습니다: {itemData.itemName}");
            Destroy(itemObject);
            return;
        }

        item.data = itemData;

        Rigidbody rb = itemObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // 바닥 재고는 플레이어가 다시 주울 수 있어야 하므로 물리 상태를 일반 월드 아이템으로 되돌림
            rb.isKinematic = false;
        }

        Collider col = itemObject.GetComponent<Collider>();

        if (col != null)
        {
            //복원된 바닥 아이템은 다시 상호작용 대상이 되어야 하므로 collider를 켬
            col.enabled = true;
        }
    }

    /// <summary>
    /// 아이템이 저장 대상인 바닥에 떨어져 있는 아이템인지 판단
    /// 진열대 슬롯에 진열된 아이템과 플레이어가 들고 있는 아이템은 저장 대상에서 제외
    /// </summary>
    public bool IsFloorStockItem(Item item, PlayerInventory playerInventory)
    {
        if (item == null)
        {
            return false;
        }

        if (playerInventory != null && playerInventory.currentItem == item)
        {
            return false;
        }

        if (item.GetComponentInParent<ShelfSlot>() != null)
        {
            return false;
        }

        if (item.GetComponentInParent<PlayerInventory>() != null)
        {
            return false;
        }

        return true;
    }
}
