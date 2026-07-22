using UnityEngine;

/// <summary>
/// 플레이어가 현재 손에 들고 있는 아이템과 아이템 데이터를 관리
/// 월드 아이템 흭득 및 데이터 기반 아이템 생성 처리를 담당
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    public Transform hand;

    public Item currentItem;
    public ItemData currentItemData;
    public GameObject holdItem;

    public Transform holdPoint;

    public int currentItems = 0;
    public int maxItems = 10;

    /// <summary>
    /// 수량형 아이템 보유량을 최대 한도를 넘지 않는 범위에서 증가시킴
    /// 외부 시스템에서 플레이어에게 일정 수량의 아이템을 지급할 때 호출
    /// </summary>
    public void AddItems(int amount)
    {
        // 한 번에 많은 수량이 들어와도 설정된 최대 보유량을 초과하지 않도록 제한
        currentItems = Mathf.Min(currentItems + amount, maxItems);
    }

    /// <summary>
    /// ItemData의 프리팹을 손 위치에 생성하고 현재 보유 아이템으로 등록
    /// 반품 구역처럼 월드 Item이 아닌 데이터만 전달받아 아이템을 들 때 호출
    /// </summary>
    public void SetItem(ItemData data)
    {      
        if (holdItem != null)
        {
            Destroy(holdItem);
        }

        GameObject obj = Instantiate(data.prefabs, holdPoint);

        Item item = obj.GetComponent<Item>();

        Rigidbody rb = item.GetComponent<Rigidbody>();

        // 손에 든 아이템이 물리력에 의해 흔들리거나 떨어지지 않도록 고정
        rb.isKinematic = true;
        currentItem = item;
        currentItemData = data;
    }

    /// <summary>
    /// 현재 보유 아이템 참조를 초기화하고 holdItem에 등록된 오브젝트가 있으면 제거
    /// 아이템을 진열대에 놓거나 소비하여 손을 비워야 할 때 호출
    /// </summary>
    public void ClearItem()
    {
        currentItemData = null;
        currentItem = null;

        if (holdItem != null)
        {
            Destroy(holdItem);
        }
    }

    /// <summary>
    /// 요청 수량과 현재 보유량 중 더 작은 값만 차감하고 실제 차감된 수량을 반환
    /// 아이템 사용이나 다른 보관 장소로의 이동으로 수량을 소비할 때 호출
    /// </summary>
    public int RemoveItems(int amount)
    {
        // 보유량보다 많이 요청해도 수량이 음수가 되지 않도록 실제 제거량을 제한
        int remove = Mathf.Min(amount, currentItems);
        currentItems -= remove;
        return remove;
    }

    /// <summary>
    /// 월드에 존재하지 Item 오브젝트를 현재 보유 아이템으로 등록하고 손의 자식으로 이동시킴
    /// 플레이어가 아이템 줍기를 실행할 때 호출
    /// </summary>
    public void PickUpItem(Item item)
    {
        // 한 번에 하나의 아이템만 들 수 있으므로 이미 들고 있다면 새로운 흭득을 취소
        if (currentItem != null)
        {
            return;
        }

        currentItem = item;
        currentItemData = item.data;   

        Rigidbody rb = item.GetComponent<Rigidbody>();
        Collider col = item.GetComponent<Collider>();

        if (rb != null)
        {
            // 손에 든 동안 월드 물리의 영향을 받지 않도록 Rigidbody를 고정
            rb.isKinematic = true;
            //rb.linearVelocity = Vector3.zero;
            //rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
        {
            //플레이어와 충돌하거나 다시 상호작용 대상으로 감지되지 않도록 Colider를 비활성화
            col.enabled = false;
        }

        // 손 위치를 기준으로 정확히 배치되도록 부모와 로컬 위치,회전을 초기화
        item.transform.SetParent(hand);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }
}
