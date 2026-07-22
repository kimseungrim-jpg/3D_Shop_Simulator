using UnityEngine;

/// <summary>
/// 진열대의 개별 상품 배치 위치를 나타냄
/// 상품의 배치 여부와 진열,회수 처리를 관리
/// </summary>
public class ShelfSlot : MonoBehaviour
{
    public Item currentItem;

    /// <summary>
    /// 현재 슬롯에 상품이 배치되어 있지 않은지 확인
    /// </summary>
    public bool IsEmpty()
    {
        return currentItem == null;
    }

    /// <summary>
    /// 비어 있는 슬롯에 상품을 등록하고 슬롯 위치에 맞춰 배치
    /// </summary>
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

    /// <summary>
    /// 슬롯의 상품 오브젝트를 제거하고 복원 가능한 ItemData를 반환
    /// </summary>
    /// <returns></returns>
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
