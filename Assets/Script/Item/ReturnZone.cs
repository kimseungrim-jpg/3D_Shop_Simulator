
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
        if(storedItems.Count == 0) return null;

        ItemData item = storedItems[0];
        storedItems.RemoveAt(0);
        return item;
    }

    /// <summary>
    /// 플레이어 상호작용 UI 안내 문구 갱신할 때 호출
    /// </summary>
    public string GetInteractText()
    {
        return storedItems.Count > 0 ? "E - 아이템 가져가기" : "";
    }

    /// <summary>
    /// 반품 구역과 상호작용할 때 호출되는 함수
    /// 현재 처리는 플레이어 측에서 담당하므로 비워둠
    /// </summary>
    public void Interact()
    {
        // Player에서 처리 (비워둬도 OK)
    }
}