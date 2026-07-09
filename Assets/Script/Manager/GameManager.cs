using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static GameManager instance;
    public FadeMessageUI fadeMessage;

    public int money = 30000;

    public enum GameState
    {
        Preparing,
        Open,
        Closing
    }

    public GameState currentState;

    public float prepareTime = 20f;
    public float openTime = 60f;
    public int day = 1;

    float timer;

    public int dailySales = 0;
    public int purchaseCost = 0;
    public int maintenanceCost = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UIManager.Instance.UpdateMoneyUI(money);
        currentState = GameState.Preparing;
        timer = prepareTime;
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

    void StartNextDay()
    {
        day++;
        currentState = GameState.Preparing;
        timer = prepareTime;
    }

    public void OnResultClosed()
    {
        StartNextDay();
    }

    void ShowResult()
    {
        UIManager.Instance.ShowResultUI(
                dailySales,
                purchaseCost,
                maintenanceCost
            );
    }

    void ForceAllCustomersLeave()
    {
        CustomerAI[] customers = FindObjectsByType<CustomerAI>(FindObjectsSortMode.None);

        foreach (var c in customers)
        {
            c.ForceLeave();
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;

        if (amount > 0)
        {
            dailySales += amount;
        }

        UIManager.Instance.UpdateMoneyUI(money);
    }
}
