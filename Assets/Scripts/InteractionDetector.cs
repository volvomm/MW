using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null; //Closest Interactable 
    private SpriteOutlineController currentOutline = null; // Outline Controller

    public GameObject interactionIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactableInRange?.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            ClearCurrentOutline();

            interactableInRange = interactable;
            interactionIcon.SetActive(true);

            // if there is no current outline and needs to show a new one.
            if (collision.TryGetComponent(out SpriteOutlineController outline))
            {
                currentOutline = outline;
                currentOutline.SetVisible(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            ClearCurrentOutline();

            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }

    private void ClearCurrentOutline() // if the object already got the outline
    {
        if (currentOutline != null)
        {
            currentOutline.SetVisible(false);
            currentOutline = null;
        }
    }

}
