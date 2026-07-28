using UnityEngine;

public class FreeMotherCat : MonoBehaviour, IInteractable
{
    public InventoryItemData basementKey;

    public GameObject cage;
    public GameObject motherCat;

    public Transform motherCatFrontPosition;

    [Header("Dialogue")]
    [SerializeField]
    private MotherCatRescueDialogue rescueDialogue;

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

        if (rescueDialogue != null)
        {
            rescueDialogue.BeginDialogue();
        }
        else
        {
            Debug.LogWarning(
                "The Rescue Dialogue field has not been assigned on FreeMotherCat."
            );
        }
    }
}