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

    /// <summary>
    /// 현재 슬롯에 올라가 있는 아이템 데이터를 반환
    /// 진열대 상태를 저장할 때 Shelf가 호출
    /// </summary>
    public ItemData GetCurrentItemData()
    {
        if (currentItem != null)
        {
            return currentItem.data;
        }

        // currentItem 참조는 없지만 자식 오브젝트가 남아있는 경우를 대비합니다.
        // 저장 직전 상태가 꼬였을 때도 최대한 실제 배치 상태를 기준으로 데이터를 찾기 위한 방어 코드
        Item childItem = GetComponentInChildren<Item>();

        if (childItem == null)
        {
            return null;
        }

        return childItem.data;
    }

    /// <summary>
    /// 저장 데이터를 적용하기 전에 슬롯에 남아있는 기존 아이템 오브젝트를 제거
    /// 이어하기 로드 시 중복 생성되는 것을 막기 위해 호출
    /// </summary>
    public void ClearSlot()
    {
        if (currentItem != null)
        {
            Destroy(currentItem.gameObject);
            currentItem = null;
            return;
        }

        Item childItem = GetComponentInChildren<Item>();

        if (childItem != null)
        {
            Destroy(childItem.gameObject);
        }

        currentItem = null;
        
    }

    /// <summary>
    /// 저장 데이터에서 찾은 ItemData를 기준으로 슬롯에 아이템 오브젝트를 다시 생성
    /// 이어하기 로드 시 Shelf가 호출
    /// </summary>
    public void RestoreItem(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("[ShelfSolt] 복원할 ItemData가 없습니다.");
            return;
        }

        if (itemData.prefabs == null)
        {
            Debug.LogWarning($"[ShelfSlot] {itemData.itemName}의 프리팹이 없습니다.");
            return;
        }

        ClearSlot();

        GameObject itemObject = Instantiate(itemData.prefabs, transform);
        itemObject.transform.localPosition = Vector3.zero;
        itemObject.transform.localRotation = Quaternion.identity;

        Item item = itemObject.GetComponent<Item>();

        if (item == null)
        {
            Debug.LogWarning($"[ShelfSlot] 복원된 프리팹에 Item 컴포넌트가 없습니다: {itemData.itemName}");
            Destroy(itemObject);
            return;
        }

        item.data = itemData;

        currentItem = item;

        Rigidbody rb = itemObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Collider col = itemObject.GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }
    }
}
