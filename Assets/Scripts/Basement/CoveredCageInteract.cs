using UnityEngine;

public class CoveredCageInteract : MonoBehaviour, IInteractable
{
    public RevealMotherCat revealManager;

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (revealManager != null)
        {
            revealManager.Interact();
        }
    }
}