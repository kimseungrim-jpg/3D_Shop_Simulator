using UnityEngine;

/// <summary>
/// 아이템의 이름, 구매, 판매 가격, 월드 생성용 프리팹을 보관하는 데이터 에셋
/// 저장/ 로드 시에는 ScriptableObject 참조를 직접 저장하지 않고 itemID를 통해 다시 찾아옴
/// </summary>
[CreateAssetMenu(menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("저장/로드 ID")]
    public string itemID;

    [Header("상품 정보")]
    public string itemName;
    public int buyPrice;
    public int sellPrice;

    [Header("상품 프리팹")]
    public GameObject prefabs;
}
