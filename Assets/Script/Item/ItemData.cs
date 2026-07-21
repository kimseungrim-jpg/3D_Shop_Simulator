using UnityEngine;

/// <summary>
/// 아이템의 이름, 구매, 판매 가격, 월드 생성용 프리팹을 보관하는 데이터 에셋
/// 여러 아이템 오브젝트와 상점 시스템이 동일한 정보를 공유할 수 있도록
/// 런타임 동작과 분리된 SO(ScriptableObject)형태로 관리
/// </summary>
[CreateAssetMenu(menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int buyPrice;
    public int sellPrice;
    public GameObject prefabs;
}
