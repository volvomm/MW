using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    private SpriteOutlineController currentOutline = null;
    private FirstTimeInteractable firstTimeInRange = null;

    [Header("Interaction Icon")]
    public GameObject interactionIcon;

    private void Start()
    {
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (interactableInRange == null)
            return;

        // Keep the original interaction behaviour.
        // This allows E to start AND continue dialogue.
        interactableInRange.Interact();

        // Once E has actually been pressed on this interactable,
        // it is no longer considered new.
        if (firstTimeInRange != null &&
            !firstTimeInRange.HasBeenInteractedWith)
        {
            firstTimeInRange.MarkAsInteracted();

            if (interactionIcon != null)
            {
                interactionIcon.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interactable = FindInteractable(collision);

        if (interactable == null)
            return;

        if (!interactable.CanInteract())
            return;

        ClearCurrentOutline();

        interactableInRange = interactable;

        currentOutline = FindOutlineController(collision);

        if (currentOutline != null)
        {
            currentOutline.SetVisible(true);
        }

        // Find the FirstTimeInteractable belonging to this
        // interaction, even if the hierarchy is different.
        firstTimeInRange = FindFirstTimeInteractable(collision);

        if (firstTimeInRange != null &&
            !firstTimeInRange.HasBeenInteractedWith)
        {
            if (interactionIcon != null)
            {
                interactionIcon.SetActive(true);
            }
        }
        else
        {
            if (interactionIcon != null)
            {
                interactionIcon.SetActive(false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable = FindInteractable(collision);

        if (interactable == null)
            return;

        if (interactable != interactableInRange)
            return;

        ClearCurrentOutline();

        interactableInRange = null;
        firstTimeInRange = null;

        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
    }

    private IInteractable FindInteractable(Collider2D collision)
    {
        if (collision == null)
            return null;

        // 1. Exact GameObject
        IInteractable interactable =
            collision.GetComponent<IInteractable>();

        if (interactable != null)
            return interactable;

        // 2. Parent
        interactable =
            collision.GetComponentInParent<IInteractable>();

        if (interactable != null)
            return interactable;

        // 3. Children
        interactable =
            collision.GetComponentInChildren<IInteractable>(true);

        return interactable;
    }

    private FirstTimeInteractable FindFirstTimeInteractable(
        Collider2D collision)
    {
        if (collision == null)
            return null;

        // 1. Exact GameObject
        FirstTimeInteractable firstTime =
            collision.GetComponent<FirstTimeInteractable>();

        if (firstTime != null)
            return firstTime;

        // 2. Parent
        firstTime =
            collision.GetComponentInParent<FirstTimeInteractable>();

        if (firstTime != null)
            return firstTime;

        // 3. Children
        firstTime =
            collision.GetComponentInChildren<FirstTimeInteractable>(true);

        return firstTime;
    }

    private SpriteOutlineController FindOutlineController(
        Collider2D collision)
    {
        if (collision == null)
            return null;

        SpriteOutlineController outline =
            collision.GetComponent<SpriteOutlineController>();

        if (outline != null)
            return outline;

        outline =
            collision.GetComponentInParent<SpriteOutlineController>();

        if (outline != null)
            return outline;

        outline =
            collision.GetComponentInChildren<SpriteOutlineController>(true);

        return outline;
    }

    private void ClearCurrentOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.SetVisible(false);
            currentOutline = null;
        }
    }
}