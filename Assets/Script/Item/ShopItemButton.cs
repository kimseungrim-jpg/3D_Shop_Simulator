using UnityEngine;

public class ShopItemButton : MonoBehaviour
{
    public ItemData itemData;

    public Transform spawnPoint;

    public void BuyItem()
    {
        if (GameManager.instance.money < itemData.buyPrice)
        {
            Debug.Log("µ· ºÎÁ·!");
            return;
        }

        GameManager.instance.AddMoney(-itemData.buyPrice);
        GameManager.instance.purchaseCost += itemData.buyPrice;

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
