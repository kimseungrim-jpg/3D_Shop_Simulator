using UnityEngine;

[CreateAssetMenu(menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int buyPrice;
    public int sellPrice;
    public GameObject prefabs;
}
