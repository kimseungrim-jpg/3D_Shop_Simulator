using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 전체 진행 상태를 관리하는 매니저
/// 날짜, 돈, 영업 상태, 하루 매출/비용을 관리하고 SaveManager와 연결해 새 게임/이어하기 상태를 적용
/// </summary>
public class GameManager : MonoBehaviour
{
    
    public static GameManager instance;
    public FadeMessageUI fadeMessage;

    [Header("기본 진행 데이터")]
    public int money = 10000;
    public int day = 1;

    [Header("새 게임 기본값")]
    [SerializeField] private int startMoney = 10000;
    [SerializeField] private int startDay = 1;

    [Header("영업 시간")]
    public float prepareTime = 20f;
    public float openTime = 60f;

    [Header("하루 정산 데이터")]
    public int dailySales = 0; //하루판매량
    public int purchaseCost = 0; //구이비
    public int maintenanceCost = 0; //매장 관리비

    [Header("누적 데이터")]
    public int totalSales = 0; //현재는 테스트용 총판매비용

    public enum GameState
    {
        Preparing,
        Open,
        Closing
    }

    public GameState currentState;

    float timer;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializeSaveFlow();
    }

    private void Update()
    {
        switch (currentState)
        {
            case GameState.Preparing:
            case GameState.Open:

                timer -= Time.deltaTime;
                UIManager.Instance.UpdateTimeUI(timer, currentState, day);

                if (timer <= 0f)
                {
                    SwichState();
                }
                break;

            case GameState.Closing:
                return;
        }

    }

    /// <summary>
    /// ShopScene이 시작될 때 새 게임인지 이어하기인지 판단
    /// 메인 메뉴에서 이어하기를 눌러 들어온 경우 SaveManager의 대기 데이터를 적용
    /// </summary>
    private void InitializeSaveFlow()
    {
        if (SaveManager.instance != null && SaveManager.instance.HasPendingLoadData)
        {
            SaveData loadData = SaveManager.instance.ConsumePendingLoadData();
            ApplyLoadedData(loadData);
            return;
        }

        StartNewGameState();
    }

    /// <summary>
    /// 새 게임 상태를 초기화
    /// 새 게임 버튼을 눌러 ShopScene에 진입했거나 저장 데이터 없이 
    /// </summary>
    private void StartNewGameState()
    {
        day = startDay;
        money = startMoney;

        dailySales = 0;
        purchaseCost = 0;
        maintenanceCost = 0;

        currentState = GameState.Preparing;
        timer = prepareTime;

        UIManager.Instance.UpdateMoneyUI(money);
        UIManager.Instance.UpdateTimeUI(timer, currentState, day);

        Debug.Log($"[GameManager] 새 게임 시작 / Day : {day}, Money : {money}");

        //새 게임 시작 직후 기본 상태를 저장
        SaveCurrentGameState();
    }

    /// <summary>
    /// 저장 파일에서 불러온 데이터를 현재 게임 상태에 적용
    /// 메인 메뉴의 이어하기 버튼으로 ShopScene에 들어왔을 때 호출
    /// </summary>
    private void ApplyLoadedData(SaveData savedata)
    {
        if (savedata == null)
        {
            Debug.LogWarning("[GameManager] 불러온 저장 데이터가 없어 새 게임으로 시작합니다.");
            StartNewGameState();
            return;
        }

        day = savedata.day;
        money = savedata.money;

        dailySales = savedata.dailySales;
        purchaseCost = savedata.purchaseCost;
        maintenanceCost= savedata.maintenanceCost;
        totalSales = savedata.totalSales;

        currentState = GameState.Preparing;
        timer = prepareTime;

        UIManager.Instance.UpdateMoneyUI(money);
        UIManager.Instance.UpdateTimeUI(timer, currentState, day);

        ApplyLoadedShelves(savedata.shelfSlots);

        Debug.Log($"[GameManager] 저장 데이터 적용 완료 / Day: {day}, Money: {money}"); 
    }

    /// <summary>
    /// 저장 데이터에 포함된 진열대 슬롯 상태를 현재 씬의 Shelf 오브젝트들에 적용
    /// </summary>
    private void ApplyLoadedShelves(List<ShelfSlotSaveData> shelfSlotSaveDataList)
    {
        Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);

        foreach (Shelf shelf in shelves)
        {
            shelf.ApplyLoadedShelfData(shelfSlotSaveDataList);
        }
    }

    /// <summary>
    /// 현재 GameManager 상태를 SaveData로 변환한 뒤 SaveManager에 저장을 요청
    /// 새 게임 시작 전후, 다음 날 시작 전후, 하루 종료 저장 시점에서 호출
    /// </summary>
    private void SaveCurrentGameState()
    {
        if (SaveManager.instance == null)
        {
            Debug.LogWarning("[GameManager] SaveManager가 없어 저장할 수 없습니다.");
            return;
        }

        SaveData saveData = CreateSaveDataFromCurrentState();
        SaveManager.instance.SaveGame(saveData);
    }

    /// <summary>
    /// 현재 GameManager가 관리 중인 날짜, 돈, 매출, 비용 데이터를 SaveData로 묶어 반환
    /// </summary>
    private SaveData CreateSaveDataFromCurrentState()
    {
        SaveData saveData = new SaveData();

        saveData.day = day;
        saveData.money = money;

        saveData.dailySales = dailySales;
        saveData.purchaseCost = purchaseCost;
        saveData.maintenanceCost = maintenanceCost;
        saveData.totalSales = totalSales;

        Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);

        foreach (Shelf shelf in shelves)
        {
            saveData.shelfSlots.AddRange(shelf.CreateShelfSlotSaveData());
        }

        return saveData;
    }


    /// <summary>
    /// 영업 상태를 전환
    /// 준비 시간 -> 영업 시간 -> 결과창 순으로 진행
    /// </summary>
    void SwichState()
    {
        switch (currentState)
        {
            case GameState.Preparing:
                currentState = GameState.Open;
                timer = openTime;

                fadeMessage.ShowMessage("장사 시작");

                break;
            case GameState.Open:
                currentState = GameState.Closing;      

                fadeMessage.ShowMessage("장사 종료", () =>
                {
                    ForceAllCustomersLeave();

                    ShowResult();
                });
                break;
        }
    }

    /// <summary>
    /// 다음 날 준비 상태로 전환
    /// 결과창을 닫은 뒤 호출, 하루 정산 데이터는 다음 날을 위해 초기화
    /// </summary>
    void StartNextDay()
    {
        day++;

        currentState = GameState.Preparing;
        timer = prepareTime;

        dailySales = 0;
        purchaseCost = 0;
        maintenanceCost = 0;

        UIManager.Instance.UpdateMoneyUI(money);
        UIManager.Instance.UpdateTimeUI(timer, currentState, day);

        Debug.Log($"[GameManager] 다음 날 시작 / Day: {day}, Money: {money}");
    }

    /// <summary>
    /// 결과창이 닫혔을 때 호출
    /// 다음 날로 전한환 뒤 현재 진행 상태를 저장
    /// </summary>
    public void OnResultClosed()
    {
        StartNextDay();

        //결과창을 닫고 다음 날로 넘어간 상태를 저장
        SaveCurrentGameState();
    }

    /// <summary>
    /// 하루 영업 결과 UI를 표시
    /// 영업 시간이 끝난 뒤 모든 손님을 내보낸 다음 호출
    /// </summary>
    void ShowResult()
    {
        UIManager.Instance.ShowResultUI(
                dailySales,
                purchaseCost,
                maintenanceCost
            );
    }

    /// <summary>
    /// 현재 씬에 남아있는 모든 손님 오브젝트에 퇴장 요청
    /// 영업 종료 시점에 호출되어 결과창 표시 전에 매장을 정리
    /// </summary>
    void ForceAllCustomersLeave()
    {
        CustomerAI[] customers = FindObjectsByType<CustomerAI>(FindObjectsSortMode.None);

        foreach (var c in customers)
        {
            c.ForceLeave();
        }
    }

    /// <summary>
    /// 보유 금액을 증가 또는 감소
    /// 판매처럼 양수 금액이 들어온 경우 일일 매출과 누적 매출에 반영
    /// </summary>
    public void AddMoney(int amount)
    {
        money += amount;

        if (amount > 0)
        {
            dailySales += amount;
            totalSales += amount;
        }

        UIManager.Instance.UpdateMoneyUI(money);
    }
}
