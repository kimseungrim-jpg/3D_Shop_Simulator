using UnityEngine;

/// <summary>
/// 월드에 배치된 아이템 오브젝트를 나타내며, 아이템 데이터와
/// 플레이어가 아이템을 줍는 상호작용을 연결
/// </summary>
public class Item : MonoBehaviour, IInteractable
{
    public ItemData data;
    
    /// <summary>
    /// 현재 씬의 플레이어 인벤토리를 찾아 이 아이템의 흭득 처리를 요청
    /// 플레이어가 아이템을 향해 상호작용을 입력을 했을 때 상호작용 시스템에서 호출
    /// </summary>
    public void Interact()
    {
        PlayerInventory player = FindAnyObjectByType<PlayerInventory>();
        //아이템 추가와 월드 오브젝트 제거 여부는 인벤토리가 판단하도록 현재 Item을 전달
        player.PickUpItem(this);
    }
    
    /// <summary>
    /// 아이템을 바라보거나 상호작용 범위에 들어왔을 때 표시할 안내 문구를 반환
    /// 플레이어 상호작용 UI가 안내 문구를 갱신할 때 호출
    /// </summary>
    public string GetInteractText()
    {
        return "E - 줍기";
    }
}
