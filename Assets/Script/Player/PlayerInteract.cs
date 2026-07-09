using UnityEngine;
using UnityEngine.UI;

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

    private void Update()
    {
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

    void CheckInteractable()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interacDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interacDistance))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                currentTarget = interactable;
                interactUI.SetActive(true);

                interactText.text = interactable.GetInteractText();

                return;
            }
        }

        currentTarget = null;
        interactUI.SetActive(false);
    }
}
