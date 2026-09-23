using UnityEngine;

public class FreeMotherCat : MonoBehaviour, IInteractable
{
    [Header("Required Item")]
    public InventoryItemData basementKey;

    [Header("Cage")]
    public GameObject cage;

    [Header("Mother Cat")]
    [Tooltip(
        "Assign the parent MotherCat GameObject here, " +
        "not MotherCatSprite."
    )]
    public GameObject motherCat;

    [Tooltip(
        "Position where Mother Cat should stand " +
        "after being freed."
    )]
    public Transform motherCatFrontPosition;

    [Header("Post Rescue Dialogue")]
    [SerializeField]
    private MotherCatRescueDialogue rescueDialogue;

    private bool alreadyFreed = false;

    public bool CanInteract()
    {
        if (alreadyFreed)
        {
            return false;
        }

        if (InventorySystem.Instance == null)
        {
            return false;
        }

        return InventorySystem.Instance.HasItem(
            basementKey
        );
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        alreadyFreed = true;

        // Move the entire Mother Cat hierarchy
        // outside the cage.
        if (motherCat != null &&
            motherCatFrontPosition != null)
        {
            motherCat.transform.position =
                motherCatFrontPosition.position;
        }

        // Remove the cage.
        if (cage != null)
        {
            cage.SetActive(false);
        }

        // Start the special conversation that happens
        // immediately after Mother Cat is freed.
        if (rescueDialogue != null)
        {
            rescueDialogue.BeginPostRescueDialogue();
        }
        else
        {
            Debug.LogWarning(
                "FreeMotherCat: " +
                "Rescue Dialogue has not been assigned."
            );
        }
    }
}