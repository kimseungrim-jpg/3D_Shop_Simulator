using UnityEngine;

public class POSMachine : MonoBehaviour, IInteractable
{
    public GameObject shopUI;

    public void Interact()
    {
        shopUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        FindAnyObjectByType<PlayerMovement>().canMove = false;
        FindAnyObjectByType<PlayerInteract>().canInteract = false;
    }

    public string GetInteractText()
    {
        return "E - 구매하기";
    }
}
