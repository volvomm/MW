using UnityEngine;

public class FirstTimeInteractable : MonoBehaviour
{
    [SerializeField] private bool hasBeenInteractedWith = false;

    public bool HasBeenInteractedWith
    {
        get { return hasBeenInteractedWith; }
    }

    public void MarkAsInteracted()
    {
        hasBeenInteractedWith = true;
    }
}