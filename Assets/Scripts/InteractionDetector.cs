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

            currentOutline = FindOutlineController(collision);

            // if there is no current outline and needs to put a new one.
            if (currentOutline != null)
            {
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

    // if there is no OutlineController on object
    private SpriteOutlineController FindOutlineController(Collider2D collision)
    {
        if (collision.TryGetComponent(out SpriteOutlineController outline))
            return outline;

        outline = collision.GetComponentInParent<SpriteOutlineController>(); // find with Parent obejct
        if (outline != null)
            return outline;

        outline = collision.GetComponentInChildren<SpriteOutlineController>(); // find with Children object
        return outline;
    }

}
