using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public ItemData data;
    
    public void Interact()
    {
        PlayerInventory player = FindAnyObjectByType<PlayerInventory>();

        player.PickUpItem(this);
    }
    
    public string GetInteractText()
    {
        return "E - ащ╠Б";
    }
}
