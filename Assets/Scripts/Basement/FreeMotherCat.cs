using UnityEngine;

public class FreeMotherCat : MonoBehaviour, IInteractable
{
    public InventoryItemData basementKey;

    public GameObject cage;
    public GameObject motherCat;

    public Transform motherCatFrontPosition;

    private bool alreadyFreed = false;

    public bool CanInteract()
    {
        return !alreadyFreed;
    }

    public void Interact()
    {
        if (alreadyFreed)
            return;

        if (InventorySystem.Instance == null)
            return;

        if (!InventorySystem.Instance.HasItem(basementKey))
            return;

        alreadyFreed = true;

        motherCat.transform.position = motherCatFrontPosition.position;

        cage.SetActive(false);
    }
}