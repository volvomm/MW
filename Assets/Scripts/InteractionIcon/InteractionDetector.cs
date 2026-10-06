using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    private SpriteOutlineController currentOutline = null;
    private FirstTimeInteractable firstTimeInRange = null;

    // Keeps the tin can dialogue active while E advances it.
    private ConditionalItemUseInteractable activeTinCanDialogue;

    // Keeps the closet lock dialogue active while E advances it.
    // This is especially important for the completion dialogue,
    // because normal world interactions are locked during the minigame.
    private ClosetLockInteractable activeClosetLockDialogue;

    // Tracks nearby interaction triggers.
    private readonly List<Collider2D> nearbyTriggers =
        new List<Collider2D>();

    [Header("Interaction Lock")]
    public bool interactionsLocked = false;

    [Header("Interaction Icon")]
    public GameObject interactionIcon;

    private void Start()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
    }

    private void Update()
    {
        // While the tin can dialogue is running,
        // hide the E indicator and keep its interaction.
        if (activeTinCanDialogue != null)
        {
            if (activeTinCanDialogue.IsDialogueActive)
            {
                if (interactionIcon != null)
                    interactionIcon.SetActive(false);

                return;
            }

            activeTinCanDialogue = null;
        }

        // While the closet lock dialogue is running,
        // hide the normal E interaction indicator.
        if (activeClosetLockDialogue != null)
        {
            if (activeClosetLockDialogue.IsDialogueActive)
            {
                if (interactionIcon != null)
                    interactionIcon.SetActive(false);

                return;
            }

            activeClosetLockDialogue = null;
        }

        RefreshNearbyInteractions();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        // -----------------------------------------------------
        // TIN CAN DIALOGUE
        // -----------------------------------------------------

        // The tin can dialogue has priority while active.
        // It must still receive E even when its
        // CanInteract() method returns false.
        if (activeTinCanDialogue != null)
        {
            if (activeTinCanDialogue.IsDialogueActive)
            {
                activeTinCanDialogue.Interact();
            }

            if (!activeTinCanDialogue.IsDialogueActive)
            {
                activeTinCanDialogue = null;
                RefreshNearbyInteractions();
            }

            return;
        }

        // -----------------------------------------------------
        // CLOSET LOCK DIALOGUE
        // -----------------------------------------------------

        // If the closet lock currently owns a dialogue,
        // allow E to continue advancing that dialogue even
        // while normal world interactions are locked.
        //
        // This allows:
        //
        // Opening dialogue
        // -> E
        // -> Minigame
        //
        // and later:
        //
        // 3/3
        // -> Completion dialogue
        // -> E
        // -> Finish unlocking.
        if (activeClosetLockDialogue != null &&
            activeClosetLockDialogue.IsDialogueActive)
        {
            activeClosetLockDialogue.Interact();

            if (!activeClosetLockDialogue.IsDialogueActive)
            {
                activeClosetLockDialogue = null;
            }

            return;
        }

        // Other dialogue, minigames and cutscenes
        // can lock normal world interactions.
        if (interactionsLocked)
            return;

        RefreshNearbyInteractions();

        // Preserve first-time tracking for existing scripts.
        // This no longer controls the E indicator.
        if (firstTimeInRange != null &&
            !firstTimeInRange.HasBeenInteractedWith)
        {
            firstTimeInRange.MarkAsInteracted();
        }

        if (interactableInRange == null)
            return;

        if (!interactableInRange.CanInteract())
            return;

        // Remember whether this is the tin can before
        // starting the interaction.
        ConditionalItemUseInteractable tinCan =
            interactableInRange as ConditionalItemUseInteractable;

        // Remember whether this is the closet lock before
        // starting the interaction.
        ClosetLockInteractable closetLock =
            interactableInRange as ClosetLockInteractable;

        // Start/advance the selected interaction.
        interactableInRange.Interact();

        // If this interaction started the tin can dialogue,
        // remember it.
        if (tinCan != null && tinCan.IsDialogueActive)
        {
            activeTinCanDialogue = tinCan;
        }

        // If this interaction started the closet-lock dialogue,
        // remember it so E can continue controlling that dialogue
        // even when the minigame locks normal interactions.
        if (closetLock != null && closetLock.IsDialogueActive)
        {
            activeClosetLockDialogue = closetLock;
        }

        RefreshNearbyInteractions();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsInteractionTrigger(collision))
            return;

        if (!nearbyTriggers.Contains(collision))
        {
            nearbyTriggers.Add(collision);
        }

        RefreshNearbyInteractions();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsInteractionTrigger(collision))
            return;

        if (!nearbyTriggers.Contains(collision))
        {
            nearbyTriggers.Add(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        nearbyTriggers.Remove(collision);
        RefreshNearbyInteractions();
    }

    private bool IsInteractionTrigger(Collider2D collision)
    {
        if (collision == null || !collision.isTrigger)
            return false;

        return FindInteractable(collision) != null ||
               FindFirstTimeInteractable(collision) != null;
    }

    private void RefreshNearbyInteractions()
    {
        IInteractable nextInteractable = null;
        FirstTimeInteractable nextFirstTime = null;
        SpriteOutlineController nextOutline = null;

        bool canShowIcon = false;

        // Most recently entered available trigger wins.
        for (int i = nearbyTriggers.Count - 1; i >= 0; i--)
        {
            Collider2D trigger = nearbyTriggers[i];

            if (trigger == null ||
                !trigger.enabled ||
                !trigger.gameObject.activeInHierarchy)
            {
                nearbyTriggers.RemoveAt(i);
                continue;
            }

            IInteractable candidate =
                FindInteractable(trigger);

            FirstTimeInteractable firstTime =
                FindFirstTimeInteractable(trigger);

            // Don't select objects that cannot currently
            // be interacted with.
            if (candidate != null &&
                !candidate.CanInteract())
            {
                continue;
            }

            if (candidate == null && firstTime == null)
                continue;

            nextInteractable = candidate;
            nextFirstTime = firstTime;
            nextOutline = FindOutlineController(trigger);

            canShowIcon = true;
            break;
        }

        // Update the visible outline.
        if (currentOutline != nextOutline)
        {
            ClearCurrentOutline();

            currentOutline = nextOutline;

            if (currentOutline != null)
            {
                currentOutline.SetVisible(true);
            }
        }

        interactableInRange = nextInteractable;
        firstTimeInRange = nextFirstTime;

        // The E indicator appears whenever Patch
        // is inside an available interaction trigger.
        if (interactionIcon != null)
        {
            bool shouldShow =
                canShowIcon && !interactionsLocked;

            if (interactionIcon.activeSelf != shouldShow)
            {
                interactionIcon.SetActive(shouldShow);
            }
        }
    }

    private IInteractable FindInteractable(
        Collider2D collision)
    {
        if (collision == null)
            return null;

        IInteractable result =
            collision.GetComponent<IInteractable>();

        if (result == null)
        {
            result =
                collision.GetComponentInParent<IInteractable>();
        }

        if (result == null)
        {
            result =
                collision.GetComponentInChildren<IInteractable>(true);
        }

        return result;
    }

    private FirstTimeInteractable FindFirstTimeInteractable(
        Collider2D collision)
    {
        if (collision == null)
            return null;

        FirstTimeInteractable result =
            collision.GetComponent<FirstTimeInteractable>();

        if (result == null)
        {
            result =
                collision.GetComponentInParent<FirstTimeInteractable>();
        }

        if (result == null)
        {
            result =
                collision.GetComponentInChildren<FirstTimeInteractable>(
                    true);
        }

        return result;
    }

    private SpriteOutlineController FindOutlineController(
        Collider2D collision)
    {
        if (collision == null)
            return null;

        SpriteOutlineController result =
            collision.GetComponent<SpriteOutlineController>();

        if (result == null)
        {
            result =
                collision.GetComponentInParent<SpriteOutlineController>();
        }

        if (result == null)
        {
            result =
                collision.GetComponentInChildren<SpriteOutlineController>(
                    true);
        }

        return result;
    }

    private void ClearCurrentOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.SetVisible(false);
            currentOutline = null;
        }
    }

    private void OnDisable()
    {
        ClearCurrentOutline();

        nearbyTriggers.Clear();

        interactableInRange = null;
        firstTimeInRange = null;

        activeTinCanDialogue = null;
        activeClosetLockDialogue = null;

        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
    }
}