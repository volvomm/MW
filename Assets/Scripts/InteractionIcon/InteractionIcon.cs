using UnityEngine;

public class FirstTimeInteractable : MonoBehaviour
{
    [SerializeField] private bool hasBeenInteractedWith = false;

    //To set this up:
    //Make sure collider 2d is TRIGGER
    //Tag 'Interactable'

    public bool HasBeenInteractedWith
    {
        get { return hasBeenInteractedWith; }
    }

    public void MarkAsInteracted()
    {
        hasBeenInteractedWith = true;
    }
}