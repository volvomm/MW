using UnityEngine;

public class ShoeboxPickup : MonoBehaviour
{
    public InventoryItemData keyItem;
    public GameObject shoeboxObject;

    bool pickedUp = false;

    public void Interact()
    {
        if (pickedUp)
            return;

        pickedUp = true;

        bool added = InventorySystem.Instance.AddItem(keyItem);

        if (added)
        {
            shoeboxObject.SetActive(false);
        }
    }
}