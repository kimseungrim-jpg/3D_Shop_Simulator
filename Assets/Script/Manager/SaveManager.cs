using System;
using System.IO;
using UnityEngine;


/// <summary>
/// 게임 진행 데이터를 JSON 파일로 저장하고 불러오는 매니저
/// 파일 입출력만 담당
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager instance { get; private set; }

    private const string SaveFileName = "saveData.json";

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public SaveData PendingLoadData { get; private set; }

    public bool HasPendingLoadData => PendingLoadData != null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        //메인 메뉴에서 게임 씬으로 넘어가도 데이터의 저장/로드를 위해 유지
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 현재 게임 진행 데이터를 JSON 파일로 저장합니다.
    /// 영업 종료 시점, 다음 날로 넘어가기 직전, 또는 수동 저장 버튼으로 호출
    /// </summary>
    public void SaveGame(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogWarning("[SaveManager] 저장할 데이터가 없습니다.");
            return;
        }

        saveData.saveAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string json = JsonUtility.ToJson(saveData, true);

        //File.WriteAllText는 파일이 없으면 새로 만들고, 이미 있으면 기존 내용을 전부 덮어쓴다.
        File.WriteAllText(SaveFilePath, json);

        Debug.Log($"[SaveManager] 저장 완료 : {SaveFilePath}");
        Debug.Log(json);
    }

    /// <summary>
    /// 저장된 JSON 파일을 읽고 SaveData 객체로 변환
    /// 메인 메뉴의 이어하기 버튼 또는 테스트 로드에서 호출
    /// </summary>
    public bool TryLoadGame(out SaveData saveData)
    {
        saveData = null;

        if (!File.Exists(SaveFilePath))
        {
            Debug.Log("[SaveManager] 저장 파일이 없습니다.");
            return false;
        }

        string json = File.ReadAllText(SaveFilePath);
        saveData = JsonUtility.FromJson<SaveData>(json);

        if (saveData == null)
        {
            Debug.LogWarning("[SaveManager] 저장 데이터를 불러오지 못했습니다.");
            return false;
        }

        Debug.Log("[SaveManager] 불러오기 성공");
        return true;
    }

    /// <summary>
    /// 저장 파일이 존재하는지 확인
    /// 메인 메뉴의 이어하기 버튼을 활성화하거나 비활성화할 때 사용
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(SaveFilePath);
    }

    /// <summary>
    /// 기존 저장 파일을 삭제
    /// 새 게임을 시작할 때 이전 진행 데이터를 초기화하기 위해 호출
    /// </summary>
    public void DeleteSaveFile()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.Log("[SaveManager] 삭제할 저장 파일이 없습니다.");
            return;
        }

        File.Delete(SaveFilePath);
        Debug.Log("[SaveManager] 저장 파일 삭제 완료");
    }

    /// <summary>
    /// 이어하기 씬을 이동하기 전에 불러온 데이터를 임시로 보관
    /// ShopScene의 GameManager가 시작될 때 이 데이터를 받아 실제 게임 상태에 적용
    /// </summary>
    public void SetPendingLoadData(SaveData saveData)
    {
        PendingLoadData = saveData;
    }

    /// <summary>
    /// 임시 보관 중인 로드 데이터를 반환
    /// 같은 저장 데이터가 중복 적용되지 않도록 꺼낸 뒤 null로 초기화
    /// </summary>
    public SaveData ConsumePendingLoadData()
    {
        SaveData loadData = PendingLoadData;
        PendingLoadData = null;
        return loadData;
    }

    /// <summary>
    ///  저장 파일이 생성되는 실제 로컬 경로를 출력
    ///  테스트 중 JSON 파일 위치를 확인할 때 사용
    /// </summary>
    public void LogSavePath()
    {
        Debug.Log($"[SaveManager] 저장 경로 : {SaveFilePath}");
    }

#if UNITY_EDITOR
    /// <summary>
    /// JSON 저장이 정상 동작하는지 확인하기 위한 테스트 함수
    /// Unity Inspector의 Context Menu에서 직접 호출
    /// </summary>
    [ContextMenu("Test/Create Dummy Save Data")]
    private void CreateDummySaveData()
    {
        SaveData saveData = new SaveData();

        saveData.day = 3;
        saveData.money = 14500;
        saveData.totalSales = 32000;
        saveData.dailySales = 8500;

        saveData.stockItems.Add(new StockItemSaveData("apple", 12));
        saveData.stockItems.Add(new StockItemSaveData("bread", 5));

        saveData.shelfSlots.Add(new ShelfSlotSaveData("Shelf_01", 0, "apple", 4));
        saveData.shelfSlots.Add(new ShelfSlotSaveData("Shelf_01", 1, "bread", 2));

        SaveGame(saveData);
    }

    /// <summary>
    /// 저장된 JSON 파일을 다시 읽어오는 테스트 함수
    /// 저장 파일이 정상적으로 생성되었는지, 값이 다시 복원되는지 확인할 때 사용
    /// </summary>
    [ContextMenu("Test/Load Dummy Save Data")]
    private void LoadDummySaveData()
    {
        if (!TryLoadGame(out SaveData saveData))
        {
            Debug.Log("[SaveManager] 테스트 로드 실패");
            return;
        }

        Debug.Log($"[SaveManager] 날짜: {saveData.day}");
        Debug.Log($"[SaveManager] 돈: {saveData.money}");
        Debug.Log($"[SaveManager] 누적 매출: {saveData.totalSales}");
        Debug.Log($"[SaveManager] 일일 매출: {saveData.dailySales}");
        Debug.Log($"[SaveManager] 저장 시각: {saveData.saveAt}");

        foreach (StockItemSaveData stockItem in saveData.stockItems)
        {
            Debug.Log($"[SaveManager] 재고 아이템: {stockItem.itemid} / 수량: {stockItem.amount}");
        }

        foreach (ShelfSlotSaveData shelfSlot in saveData.shelfSlots)
        {
            Debug.Log($"[SaveManager] 진열대: {shelfSlot.shelfId} / 슬롯: {shelfSlot.slotIndex} / 아이템: {shelfSlot.itemId} / 수량: {shelfSlot.amount}");
        }
    }

    /// <summary>
    /// 저장 파일 경로를 확인하기 위한 테스트 함수
    /// 생성된 saveData.json을 직접 열어보고 싶을 때 사용.
    /// </summary>
    [ContextMenu("Test/Log Save File Path")]
    private void LogSaveFilePathByContextMenu()
    {
        LogSavePath();
    }

    /// <summary>
    /// 테스트용 저장 파일을 삭제
    /// 저장 파일이 없는 상태에서 이어하기 버튼 처리를 확인할 때 사용
    /// </summary>
    [ContextMenu("Test/Delete Save File")]
    private void DeleteSaveFileByContextMenu()
    {
        DeleteSaveFile();
    }
#endif
}
