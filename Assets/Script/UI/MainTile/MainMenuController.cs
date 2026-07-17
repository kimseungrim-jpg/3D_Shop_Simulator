using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// 메인 메뉴의 새 게임, 이어하기, 게임 종료 버튼을 제어하는 스크립트
/// 저장 파일 유무에 따라 이어하기 버튼 상태를 갱신, 선택한 시작 방식에 맞게 상점 씬으로 이동
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("메인 메뉴 버튼")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    [Header("넘어갈 게임 씬 이름")]
    [SerializeField] private string shopSceneName = "ShopScene";

    private void OnEnable()
    {
        //버튼 클릭 이벤트는 UI가 활성화될 떄 연결
        //OnDisable에서 해제해 씬 전환이나 오브젝트 비활성화 시 중복 호출을 방지
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnClickNewGame);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnClickContinue);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnClickQuitGame);
        }
    }

    private void Start()
    {
        UpdateContinueButton();
    }

    private void OnDisable()
    {
        //버튼 이벤트를 해제해서 메인 메뉴를 다시 열었을 때 같은 함수가 여러 번 호출되지 않게 함
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveListener(OnClickNewGame);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnClickContinue);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnClickQuitGame);
        }
    }

    /// <summary>
    /// 저장 파일 존재 여부에 따라 이어하기 버튼을 활성화하거나 비활성화
    /// 메인 메뉴가 시작될 때 호출
    /// </summary>
    private void UpdateContinueButton()
    {
        if (continueButton == null)
            return;

        bool hasSaveFile = SaveManager.instance != null && SaveManager.instance.HasSaveFile();
        continueButton.interactable = hasSaveFile;
    }

    /// <summary>
    /// 새 게임 버튼을 눌렀을 때 호출
    /// 기존 저장 파일을 삭제 후, 로드 대기 데이터를 비운 뒤 상점 씬으로 이동
    /// </summary>
    private void OnClickNewGame()
    {
        if (SaveManager.instance != null)
        {
            //새 게임은 이전 진행 데이터를 사용하지 않아야 하므로 저장 파일과 대기 데이터를 초기화
            SaveManager.instance.DeleteSaveFile();
            SaveManager.instance.SetPendingLoadData(null);
        }

        Debug.Log("[MainMenuController] 새 게임 시작");
        SceneManager.LoadScene(shopSceneName);
    }

    /// <summary>
    /// 이어하기 버튼을 눌렀을 때 호출
    /// 저장 파일을 읽고, 상점 씬의 GameManager가 사용할 수 있도록 SaveManager에 임시 보관한 뒤 씬 호출
    /// </summary>
    private void OnClickContinue()
    {
        if (SaveManager.instance == null)
        {
            Debug.LogWarning("[MainMenuController] SaveManager가 없습니다.");
            return;
        }

        if (!SaveManager.instance.TryLoadGame(out SaveData saveData))
        {
            Debug.LogWarning("[MainMenuController] 불러올 저장 데이터가 없습니다.");
            UpdateContinueButton();
            return;
        }

        //실제 데이터 적용은 상점 씬의 GameManager가 담당
        //여기서는 씬 이동 전에 읽어온 SaveData를 잠깐 보관
        SaveManager.instance.SetPendingLoadData(saveData);

        Debug.Log("[MainMenuController] 이어하기 시작");
        SceneManager.LoadScene(shopSceneName);
    }

    /// <summary>
    /// 게임 종료 버튼을 눌렀을 때 호출
    /// 빌드된 게임에서는 애플리케이션을 종료, 에디터에서는 로그만 출력
    /// </summary>
    private void OnClickQuitGame()
    {
        Debug.Log("[MainMenuController] 게임 종료 요청");
        Application.Quit();
    }
}
