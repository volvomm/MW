using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    private SpriteOutlineController currentOutline = null;
    private FirstTimeInteractable firstTimeInRange = null;

    [Header("Interaction Lock")]
    public bool interactionsLocked = false;

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

        // A dialogue/cutscene currently owns the E key.
        // Do not activate world interactables.
        if (interactionsLocked)
            return;

        // First-time interaction system.
        // This works even for interactions such as the kittens,
        // which handle E through their own scripts.
        if (firstTimeInRange != null &&
            !firstTimeInRange.HasBeenInteractedWith)
        {
            firstTimeInRange.MarkAsInteracted();

            if (interactionIcon != null)
            {
                interactionIcon.SetActive(false);
            }
        }

        // If this object uses IInteractable,
        // perform its normal interaction.
        if (interactableInRange == null)
            return;

        interactableInRange.Interact();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ---------------------------------------------
        // FIRST-TIME INTERACTION ICON
        // ---------------------------------------------

        if (collision.CompareTag("Interactable"))
        {
            FirstTimeInteractable firstTime =
    collision.GetComponent<FirstTimeInteractable>();

            if (firstTime != null)
            {
                firstTimeInRange = firstTime;

                if (!firstTimeInRange.HasBeenInteractedWith)
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
        }

        // ---------------------------------------------
        // NORMAL IINTERACTABLE SYSTEM
        // ---------------------------------------------

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
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // ---------------------------------------------
        // FIRST-TIME INTERACTION ICON
        // ---------------------------------------------

        if (collision.CompareTag("Interactable"))
        {
            FirstTimeInteractable exitingFirstTime =
     collision.GetComponent<FirstTimeInteractable>();

            if (exitingFirstTime != null &&
                exitingFirstTime == firstTimeInRange)
            {
                if (interactionIcon != null)
                {
                    interactionIcon.SetActive(false);
                }

                firstTimeInRange = null;
            }
        }

        // ---------------------------------------------
        // NORMAL IINTERACTABLE SYSTEM
        // ---------------------------------------------

        IInteractable interactable = FindInteractable(collision);

        if (interactable == null)
            return;

        if (interactable != interactableInRange)
            return;

        ClearCurrentOutline();

        interactableInRange = null;
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