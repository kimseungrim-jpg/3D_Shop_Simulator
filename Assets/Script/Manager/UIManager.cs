using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI timeText;

    public GameObject resultPanel;

    public TextMeshProUGUI salesText;
    public TextMeshProUGUI purchaseText;
    public TextMeshProUGUI maintenanceText;
    public TextMeshProUGUI profitText;
    public TextMeshProUGUI closeInfoText;

    bool canClose = false;
    float closeTimer = 30f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateMoneyUI(GameManager.instance.money);
    }

    private void Update()
    {
        if (!resultPanel.activeSelf) return;

        if (canClose)
        {
            closeTimer -= Time.deltaTime;

            closeInfoText.text = $"{Mathf.CeilToInt(closeTimer)}초 후 자동 종료\n클릭하여 닫기";

            if (Input.GetMouseButtonDown(0) && canClose)
            {
                CloseResultUI();
            }

            if (closeTimer <= 0f)
            {
                CloseResultUI();
            }
        }
    }

    public void UpdateMoneyUI(int money)
    {
        moneyText.text = money.ToString() + "WON";
    }

    public void UpdateTimeUI(float time, GameManager.GameState state, int day)
    {
        string stateText = "";

        switch (state)
        {
            case GameManager.GameState.Preparing:
                stateText = $"Day {day} OPEN : {Mathf.CeilToInt(time)}sec";
                break;
            case GameManager.GameState.Open:
                stateText = $"DAY {day} CLOSE : {Mathf.CeilToInt(time)}sec";
                break;
            case GameManager.GameState.Closing:
                stateText = $"Day {day} END";
                break;
        }

        timeText.text = stateText;
    }

    public void CloseShop(GameObject shopUI)
    {
        shopUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        FindAnyObjectByType<PlayerMovement>().canMove = true;
        FindAnyObjectByType<PlayerInteract>().canInteract = true;
    }

    public void ShowResultUI(int sales, int purchse, int maintenance)
    {
        resultPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        FindAnyObjectByType<PlayerMovement>().canMove = false;
        FindAnyObjectByType<PlayerInteract>().canInteract = false;

        canClose = false;
        closeInfoText.text = "정산 중...";

        StartCoroutine(ResultSequence(sales, purchse, maintenance));
    }

    IEnumerator ResultSequence(int sales, int purchase, int maintenance)
    {
        salesText.text = "";
        purchaseText.text = "";
        maintenanceText.text = "";
        profitText.text = "";

        int profit = sales - purchase - maintenance;

        yield return new WaitForSeconds(1f);

        salesText.text = $"판매: {sales}";
        yield return new WaitForSeconds(2f);

        purchaseText.text = $"구입비: -{purchase}";
        yield return new WaitForSeconds(2f);

        maintenanceText.text = $"매장 관리비: -{maintenance}";
        yield return new WaitForSeconds(2f);

        profitText.text = $"순수익: {profit}";

        canClose = true;
        closeTimer = 30f;

        closeInfoText.text = $"{Mathf.CeilToInt(closeTimer)}초 후 자동 종료\n클릭하여 닫기";
    }

    void CloseResultUI()
    {
        resultPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        FindAnyObjectByType<PlayerMovement>().canMove = true;
        FindAnyObjectByType<PlayerInteract>().canInteract = true;

        GameManager.instance.OnResultClosed();
    }
}
