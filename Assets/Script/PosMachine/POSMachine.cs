using UnityEngine;

/// <summary>
/// 플레이어가 상품 구매 UI를 열 수 있는 POS 상호작용 오브젝트
/// 상점 UI가 열려 있는 동안 커서 상태와 플레이어의 월드 조작을 제어
/// </summary>
public class POSMachine : MonoBehaviour, IInteractable
{
    public GameObject shopUI;

    /// <summary>
    /// 상점 UI를 열고 UI 조작이 가능하도록 커서를 해제한 뒤 플레이어 조작을 차단
    /// </summary>
    public void Interact()
    {
        shopUI.SetActive(true);

        //상점 조작을 위해서 마우스 커서를 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // UI 조작 중 플레이어가 움직이거나 화면이 회전되는 것을 멈추기 위해 차단
        FindAnyObjectByType<PlayerMovement>().canMove = false;
        FindAnyObjectByType<PlayerInteract>().canInteract = false;
    }

    public string GetInteractText()
    {
        return "E - 구매하기";
    }
}
