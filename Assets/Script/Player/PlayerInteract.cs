using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 중앙에서 Raycast로 상호작용 대상을 감지하고 안내 UI를 표시
/// 플레이어의 입력을 감지된 IInteractable 구현체의 행동으로 연결
/// </summary>
public class PlayerInteract : MonoBehaviour
{
    public float interacDistance = 3f;
    public Camera playerCamera; 

    public GameObject interactUI;
    public Text interactText;

    public bool canInteract = true;

    IInteractable currentTarget;

    PlayerInventory inventory;

    private void Start()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    /// <summary>
    /// 상호작용 가능 상태를 확인하고 대상을 탐색한 뒤 E 입력 시 상호작용을 실행
    /// </summary>
    private void Update()
    {
        //결과창이나 상점 UI가 열린 동안에는 안내 Ui를 숨기고 월드 상호작용을 차단
        if (!canInteract)
        {
            interactUI.SetActive(false);
            return;
        }

        CheckInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null)
        {
            HandleInteract(currentTarget);
        }
    }

    /// <summary>
    /// 감지된 대상의 종류에 따라 필요한 별도 처리를 수행하거나 공통 Interact를 호출
    /// E 키를 눌렀고 현재 상호작용 대상이 존재할 때 Update에서 호출
    /// </summary>
    void HandleInteract(IInteractable target)
    {
        if (target is ReturnZone returnZone)
        {
            ItemData data = returnZone.TakeItem();

            if (data != null && inventory.currentItem == null)
            {
                inventory.SetItem(data);
            }

            return;
        }

        if (target is Shelf shelf)
        {
            shelf.Interact();
            return;
        }

        target.Interact();
    }

    /// <summary>
    /// 카메라 화면 중앙에서 일정 거리만큼 RayCast하여 IInteractable 구현체를 찾음
    /// 현재 대상과 상호작용 안내 UI를 갱신
    /// </summary>
    void CheckInteractable()
    {
        // 마우스 포인터 없이 바라보는 대상을 검사
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interacDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interacDistance))
        {
            //자식 오브젝트에 있는 콜라이더가 있어도 부모에 부착된 상호작용 구현체를 찾음
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                currentTarget = interactable;
                interactUI.SetActive(true);

                interactText.text = interactable.GetInteractText();

                return;
            }
        }

        //감지된 대상이 없으면 이전 프레임의 대상이 다시 사용되지 않도록 참조와 UI를 초기화
        currentTarget = null;
        interactUI.SetActive(false);
    }
}
