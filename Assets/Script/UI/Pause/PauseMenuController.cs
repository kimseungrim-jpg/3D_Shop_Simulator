using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 상점 씬의 일시정지 메뉴를 관리
/// ESC 입력으로 게임을 멈추고, 타이틀 이동 / 옵션 메뉴 / 게임 종료를 처리
/// 저장은 담당하지 않음, 저장은 장사 종료 후 결과 창이 닫힐때 저장됨
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionPanel;

    [Header("일시정지 메뉴 버튼")]
    [SerializeField] private Button titleButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button returnGameButton;

    [Header("옵션 메뉴 버튼")]
    [SerializeField] private Button optionBackButton;

    [Header("씬 이름")]
    [SerializeField] private string titleSceneName = "MainMenuScene";

    [Header("비활성화할 플레이어 조작")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInteract playerInteract;

    private bool isPaused;

    private void Awake()
    {
        CloseAllPanels();
    }

    private void OnEnable()
    {
        if (titleButton != null)
        {
            titleButton.onClick.AddListener(ReturnToTitle);
        }

        if (optionButton != null)
        {
            optionButton.onClick.AddListener(OpenOptionMenu);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (optionBackButton != null)
        {
            optionBackButton.onClick.AddListener(CloseOptionMenu);
        }

        if (returnGameButton != null)
        {
            returnGameButton.onClick.AddListener(ResumeGame);
        }
    }

    private void OnDisable()
    {
        if (titleButton != null)
        {
            titleButton.onClick.RemoveListener(ReturnToTitle);
        }

        if (optionButton != null)
        {
            optionButton.onClick.RemoveListener(OpenOptionMenu);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
        }

        if (optionBackButton != null)
        {
            optionBackButton.onClick.RemoveListener(CloseOptionMenu);
        }

        if (returnGameButton != null)
        {
            returnGameButton.onClick.RemoveListener(ResumeGame);
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        // 옵션 메뉴가 열려 있을 때 ESC를 누르면 바로 게임으로 돌아가지 않고 일시정지 메뉴로 돌아감
        if (optionPanel != null && optionPanel.activeSelf)
        {
            CloseOptionMenu();
            return;
        }

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    /// <summary>
    /// 일시정지 메뉴를 열고 게임 진행을 멈춤
    /// ESC를 눌렀을 때 호출
    /// </summary>
    public void OpenPauseMenu()
    {
        AudioManager.Instance?.PlayPopup();

        isPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }

        SetPlayerControl(false);

        Time.timeScale = 0f;

        SetMenuCursor();
    }

    /// <summary>
    /// 일시정지 메뉴를 닫고 게임을 다시 진행
    /// 일시정지 상태에서 ESC를 다시 눌렀을 때 호출
    /// </summary>
    public void ResumeGame()
    {
        AudioManager.Instance?.PlayButtonClick();

        isPaused = false;

        CloseAllPanels();

        SetPlayerControl(true);

        Time.timeScale = 1f;

        SetGameplayCursor();
    }

    /// <summary>
    /// 옵션 메뉴 패널 ON
    /// 현재는 음량 조절 UI를 배치하기 위한 껍데기 역할
    /// </summary>
    public void OpenOptionMenu()
    {
        AudioManager.Instance?.PlayPopup();

        Debug.Log("[PauseMenuController] 옵션 버튼 클릭됨");


        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (optionPanel != null)
        {
            optionPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 옵션 메뉴에서 다시 일시정지 메뉴로 복귀
    /// 옵션 뒤로가기 버튼 또는 옵션 메뉴 상태에서 ESC를 눌렀을 때 호출
    /// </summary>
    private void CloseOptionMenu()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    /// <summary>
    /// 저장을 새로 실행하지 않고 타이틀 씬으로 이동
    /// 저장은 기존 하루 종료 저장 방식만 사용
    /// </summary>
    private void ReturnToTitle()
    {
        AudioManager.Instance?.PlayButtonClick();

        Time.timeScale = 1f;

        SetPlayerControl(true);

        SetMenuCursor();

        SceneManager.LoadScene(titleSceneName);
    }

    /// <summary>
    /// 게임을 종료
    /// 빌드에서는 애플리케이션을 종료하고, 에디터에서는 플레이 종료 대신 로그를 남김
    /// </summary>
    private void QuitGame()
    {
        AudioManager.Instance?.PlayButtonClick();

        Time.timeScale = 1f;

#if UNITY_EDITOR
        Debug.Log("[PauseMenuController] 게임 종료 요청");
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 모든 일시정지 관련 패널을 닫음
    /// 씬 시작 또는 게임 재개 시 호출
    /// </summary>
    private void CloseAllPanels()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 일시정지 중에는 플레이어 이동과 상호작용을 막음
    /// 게임 재게 시 다시 활성화
    /// </summary>
    private void SetPlayerControl(bool canControl)
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = canControl;
        }

        if (playerInteract != null)
        {
            playerInteract.enabled = canControl;
        }
    }

    /// <summary>
    /// 게임 플레이 상태의 커서 설정으로 전환
    /// 일시정지 메뉴를 닫고 다시 게임으로 돌아갈 때 호출
    /// </summary>
    private void SetGameplayCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    /// <summary>
    /// UI 메뉴 조작 상태의 커서 설정으로 전환
    /// 일시정지 메뉴나 옵션 메뉴를 열 때 호출
    /// </summary>
    private void SetMenuCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
