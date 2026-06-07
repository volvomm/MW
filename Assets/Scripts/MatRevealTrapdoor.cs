using UnityEngine;

public class MatRevealTrapdoor : MonoBehaviour, IInteractable
{
    public GameObject matObject;
    public GameObject lockedTrapdoor;

    bool revealed = false;

    public void Interact()
    {
        if (revealed)
            return;

        revealed = true;

        matObject.SetActive(false);
        lockedTrapdoor.SetActive(true);
    }

    public bool CanInteract()
    {
        return !revealed;
    }
}