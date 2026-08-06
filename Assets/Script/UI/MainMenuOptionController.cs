using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메인 메뉴 씬의 옵션 패널을 열고 닫음
/// </summary>
public class MainMenuOptionController : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject optionPanel;

    [Header("버튼")]
    [SerializeField] private Button optionButton;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        CloseOptionPanel();
    }

    private void OnEnable()
    {
        if (optionButton != null)
        {
            optionButton.onClick.AddListener(OpenOptionPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseOptionPanel);
        }
    }

    private void OnDisable()
    {
        if (optionButton != null)
        {
            optionButton.onClick.RemoveListener(OpenOptionPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseOptionPanel);
        }
    }

    /// <summary>
    /// 옵션 버튼을 눌렀을 때 옵션 버튼을 표시
    /// </summary>
    private void OpenOptionPanel()
    {
        AudioManager.Instance?.PlayPopup();

        if (optionPanel != null)
        {
            optionPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 옵션 창을 닫고 기존 화면으로 돌아감
    /// </summary>
    private void CloseOptionPanel()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }
    }
}
