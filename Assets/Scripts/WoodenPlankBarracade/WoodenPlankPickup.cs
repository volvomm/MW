using UnityEngine;

public class WoodenPlankPickup : MonoBehaviour, IInteractable
{
    [Header("Inventory")]
    [SerializeField] private InventoryItemData woodenPlankItem;

    [Header("World Objects")]
    [SerializeField] private GameObject doublePlanks;
    [SerializeField] private GameObject singlePlank;

    private bool plankTaken;

    public bool CanInteract()
    {
        // Patch can only take the plank AFTER
        // the Devil Dog has disappeared into the closet.
        return StoryProgress.DevilDogTrapdoorSequenceFinished
               && !plankTaken;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning(
                "WoodenPlankPickup: InventorySystem.Instance was not found."
            );

            return;
        }

        if (woodenPlankItem == null)
        {
            Debug.LogWarning(
                "WoodenPlankPickup: Wooden Plank Item has not been assigned."
            );

            return;
        }

        bool added =
            InventorySystem.Instance.AddItem(woodenPlankItem);

        // If the inventory is full, do not change the planks.
        if (!added)
        {
            return;
        }

        plankTaken = true;

        // Refresh the inventory UI.
        InventoryUIController inventoryUI =
            FindFirstObjectByType<InventoryUIController>();

        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        // Remove the double-plank pile.
        if (doublePlanks != null)
        {
            doublePlanks.SetActive(false);
        }

        // Show the one remaining plank.
        if (singlePlank != null)
        {
            singlePlank.SetActive(true);
        }
    }
}