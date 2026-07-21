using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 소지금과 영업 시간 HUD를 갱신, 상점 UI 및 하루 영업 결과창의
/// 표시와 플레이어 입력 상태를 관리
/// </summary>
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

    /// <summary>
    /// 결과창을 닫을 수 있는 시점부터 자동 종료 시간을 갱신
    /// 마우스 클릭 또는 시간 만료에 따라 결과창을 종료
    /// Unity가 UIManager 오브젝트가 활성화된 동안 매 플레임 호출
    /// </summary>
    private void Update()
    {
        //결과창이 보이지 않을 때는 종료 입력과 타이머를 처리하지 않음
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

    /// <summary>
    /// 전달받은 소지금을 화면의 금액 텍스트에 반영
    /// GameManager에서 플레이어의 돈이 변경될 때 호출
    /// </summary>
    public void UpdateMoneyUI(int money)
    {
        moneyText.text = money.ToString() + "WON";
    }

    /// <summary>
    /// 현재 날짜와 게임 상태에 맞춰 영업 준비, 영업 종료까지의 시간을 표시
    /// GameManager에서 플레이어의 돈이 변경될 때 호출
    /// </summary>
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

    /// <summary>
    /// 상점 UI를 닫고 커서를 다시 잠근 뒤 플레이어의 이동과 상호작용을 허용
    /// 상점 UI의 닫기 버튼을 눌렀을 때 호출
    /// </summary>
    /// <param name="shopUI"></param>
    public void CloseShop(GameObject shopUI)
    {
        shopUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        FindAnyObjectByType<PlayerMovement>().canMove = true;
        FindAnyObjectByType<PlayerInteract>().canInteract = true;
    }

    /// <summary>
    /// 하루 영업 결과창을 열고 정산 항목을 순서대로 표시하는 코루틴을 시작
    /// 영업 종료 후 GameManager가 판매액 구입비 관리비 정산을 요청할 때 호출
    /// </summary>
    public void ShowResultUI(int sales, int purchse, int maintenance)
    {
        resultPanel.SetActive(true);

        //결과 UI를 조작할 수 있도록 커서를 해제하고 정산 중에는 월드 조작을 차단
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        FindAnyObjectByType<PlayerMovement>().canMove = false;
        FindAnyObjectByType<PlayerInteract>().canInteract = false;

        canClose = false;
        closeInfoText.text = "정산 중...";

        StartCoroutine(ResultSequence(sales, purchse, maintenance));
    }

    /// <summary>
    /// 판매액, 구입비, 관리비, 순수익을 일정한 간격으로 차례대로 공개
    /// ShowResultUI에서 결과창을 연 직후 코루틴으로 실행
    /// </summary>
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

    /// <summary>
    /// 영어 결과창을 닫고 플레이어 조작을 복원
    /// 정산 공개 후 플레이어가 클릭하거나 자동 종료 시간이 만료되었을 때 호출
    /// </summary>
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
