using UnityEngine;

public class MatRevealTrapdoor : MonoBehaviour, IInteractable
{
    [Header("Objects")]
    [SerializeField] private GameObject matObject;
    [SerializeField] private GameObject lockedTrapdoorObject;

    private bool revealed = false;

    public bool CanInteract()
    {
        return !revealed;
    }

    public void Interact()
    {
        if (revealed)
            return;

        revealed = true;

        if (matObject != null)
            matObject.SetActive(false);

        if (lockedTrapdoorObject != null)
            lockedTrapdoorObject.SetActive(true);
    }
}