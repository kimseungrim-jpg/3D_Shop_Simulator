using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// itemID를 기준으로 itemData를 찾아주는 아이템 데이터베이스
/// 저장 파일에는 itemID만 남기고, 로드 시 이 데이터베이스를 통해 실제 ItemData를 다시 찾습니다.
/// </summary>
public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    [Header("게임에서 사용하는 전체 아이템 데이터")]
    [SerializeField] private List<ItemData> allItems = new List<ItemData>();

    private readonly Dictionary<string, ItemData> itemTable = new Dictionary<string, ItemData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildItemTable();
    }

    /// <summary>
    /// 등록된 ItemData목록을 itemID 기준 Dictionary로 변환
    /// 씬 시작 시 호출 -> 저장 데이터의 itemData로 복원하기 위해 사용
    /// </summary>
    private void BuildItemTable()
    {
        itemTable.Clear();

        foreach (ItemData itemData in allItems)
        {
            if (itemData == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(itemData.itemID))
            {
                Debug.LogWarning($"[ItemDatabase] itemData가 비어있는 ItemData가 있습니다: {itemData.name}");
                continue;
            }

            if (itemTable.ContainsKey(itemData.itemID))
            {
                Debug.LogWarning($"[ItemDatabase] 중복 itemID발견: {itemData.itemID}");
                continue;
            }

            itemTable.Add(itemData.itemID, itemData);
        }

        Debug.Log($"[ItemDatabase] 아이템 데이터 등록 완료 / Count: {itemTable.Count}");
    }

    /// <summary>
    /// itemID를 기준으로 ItemData를 찾음
    /// 저장 데이터를 불러와 재고나 진열대를 복원할 때 호출
    /// </summary>
    public bool TryGetItemData(string itemID, out ItemData itemData)
    {
        itemData = null;

        if (string.IsNullOrWhiteSpace(itemID))
        {
            Debug.LogWarning("[ItemDatabase] 비어있는 itemID로 아이템을 찾으려고 했습니다.");
            return false;
        }

        if (!itemTable.TryGetValue(itemID, out itemData))
        {
            Debug.LogWarning($"[ItemDatabase] itemID에 해당하는 ItemData를 찾을 수 없습니다: {itemID}");
            return false;
        }

        return true;
    }


#if UNITY_EDITOR
    /// <summary>
    /// ItemDatabase가 itemId로 ItemData를 정상적으로 찾는지 확인하기 위한 테스트 함수입니다.
    /// Inspector의 Context Menu에서 호출합니다.
    /// </summary>
    [ContextMenu("Test/Find First Item")]
    private void TestFindFirstItem()
    {
        if (allItems.Count <= 0 || allItems[0] == null)
        {
            Debug.LogWarning("[ItemDatabase] 테스트할 ItemData가 없습니다.");
            return;
        }

        string testItemId = allItems[0].itemID;

        if (TryGetItemData(testItemId, out ItemData itemData))
        {
            Debug.Log($"[ItemDatabase] 테스트 성공 / itemId: {testItemId}, itemName: {itemData.itemName}");
        }
    }
#endif
}
