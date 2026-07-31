
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 결제하지 않고 떠난 손님의 상품을 임시 보관
/// 플레이어가 반품 상품을 하나씩 회수할 수 있도록 제공
/// </summary>
public class ReturnZone : MonoBehaviour, IInteractable
{
    public List<ItemData> storedItems = new List<ItemData>();

    /// <summary>
    /// 손님이 반납한 상품 목록을 기존 보관 목록 뒤에 추가
    /// CustomerAI가 결제하지 않은 상품을 반납하고 매장을 떠날 때 호출
    /// </summary>
    public void AddItems(List<ItemData> items)
    {
        storedItems.AddRange(items);
    }

    /// <summary>
    /// 가장 먼저 보관된 상품 하나를 목록에서 제거하고 반환
    /// 플레이어의 반품 상품 회수 로직에서 실제 아이템을 가져갈 때 호출
    /// </summary>
    public ItemData TakeItem()
    {
        if (storedItems.Count == 0)
        {
            return null;
        }

        ItemData item = storedItems[0];
        storedItems.RemoveAt(0);
        return item;
    }

    /// <summary>
    /// 플레이어 상호작용 UI 안내 문구 갱신할 때 호출
    /// 반납함에 아이템이 있으면 가장 먼저 꺼낼 아이템과 남은 총 수량을 함꼐 표시
    /// </summary>
    public string GetInteractText()
    {
        if (storedItems.Count == 0)
        {
            return "반납함이 비어있습니다.";
        }

        ItemData firstItem = storedItems[0];

        if (storedItems.Count == 1)
        {
            if (firstItem == null)
            {
                return "E - 아이템 가져가기\n알 수 없는 아이템";
            }

            return $"E - 아이템 가져가기\n{firstItem.itemName}";
        }

        if (firstItem == null)
        {
            return $"E - 아이템 가져가기\n알 수 없는 아이템 / 남은 수량 {storedItems.Count}";
        }

        return $"E - 아이템 가져가기\n{firstItem.itemName} / 남은 수량 {storedItems.Count}";
    }

    /// <summary>
    /// 반품 구역과 상호작용할 때 호출되는 함수
    /// 현재 처리는 플레이어 측에서 담당하므로 비워둠
    /// </summary>
    public void Interact()
    {
        // Player에서 처리 (비워둬도 OK)
    }

    /// <summary>
    /// 반납상자에 보관된 첫 번째 아이템을 제거하지 않고 확인만 함
    /// 플레이어가 실제로 아이템을 들 수 있는지 검사하기 전에 사용
    /// </summary>
    public ItemData PeekItem()
    {
        if (storedItems.Count == 0)
        {
            return null;
        }
        
        return storedItems[0];
    }

    /// <summary>
    /// 반납상자에 보관된 첫 번째 아이템을 실제로 제거
    /// 플레이어가 아이템을 손에 드는 데 성공한 뒤 호출
    /// </summary>
    public void RemoveFirstItem()
    {
        if (storedItems.Count == 0)
        {
            return;
        }

        storedItems.RemoveAt(0);
    }

    /// <summary>
    /// 현재 반납함에 보관된 아이템 목록을 저장 데이터로 변환
    /// storedItems의 순서를 그대로 저장하기 위해 아이템 하나당 저장 데이터 하나를 생성
    /// </summary>
    public List<ReturnZoneItemSaveData> CreateReturnZoneSaveData()
    {
        List<ReturnZoneItemSaveData> saveDataList = new List<ReturnZoneItemSaveData>();

        foreach (ItemData itemData in storedItems)
        {
            if (itemData == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(itemData.itemID))
            {
                Debug.LogWarning($"[ReturnZone] ItemID가 없어 반납할 아이템을 저장할 수 없습니다.");
                continue;
            }

            saveDataList.Add(new ReturnZoneItemSaveData(itemData.itemID));
        }

        Debug.Log($"[ReturnZone] 반납함 저장 개수: {saveDataList.Count}");

        return saveDataList;
    }

    /// <summary>
    /// 저장 데이터에 있던 반납함 아이템 목록을 현재 반납함에 복원
    /// 저장된 순서대로 storedItems에 다시 추가하여 기존 회수 순서를 유지
    /// </summary>
    public void ApplyLoadedReturnZoneData(List<ReturnZoneItemSaveData> saveDataList)
    {
        storedItems.Clear();

        if (saveDataList == null)
        {
            return;
        }

        if (ItemDatabase.Instance == null)
        {
            Debug.LogWarning("[ReturnZone] ItemDatabase가 없어 반납함 아이템을 복원할 수 없습니다.");
            return;
        }

        foreach (ReturnZoneItemSaveData saveData in saveDataList)
        {
            if (saveData == null)
            {
                continue;
            }

            if (!ItemDatabase.Instance.TryGetItemData(saveData.ItemId, out ItemData itemData))
            {
                continue;
            }

            storedItems.Add(itemData);
        }

        Debug.Log($"[ReturnZone] 반납할 복원 완료 / Count: {storedItems.Count}");
    }
}