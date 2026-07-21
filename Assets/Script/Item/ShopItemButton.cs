using UnityEngine;

public class ShopItemButton : MonoBehaviour
{
    public ItemData itemData;

    public Transform spawnPoint;

    /// <summary>
    /// 소지금이 충분하면 상품 가격을 차감하고 구매한 아이템 프리팹을 생성
    /// 상점 상품 버튼의 OnClick 이벤트에 연결되어 버튼을 눌렀을 때 호출
    /// </summary>
    public void BuyItem()
    {
        if (GameManager.instance.money < itemData.buyPrice)
        {
            Debug.Log("돈 부족!");
            return;
        }

        GameManager.instance.AddMoney(-itemData.buyPrice);
        GameManager.instance.purchaseCost += itemData.buyPrice;

        //연속 구매한 상품들이 전확히 같은 위치에 겹치지 않도록 수평 위치를 조금씩 분산
        Vector3 offset = Random.insideUnitSphere * 0.5f;
        offset.y = 0;

        GameObject obj = Instantiate(itemData.prefabs, spawnPoint.position + offset, Quaternion.identity);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }
    }
}
