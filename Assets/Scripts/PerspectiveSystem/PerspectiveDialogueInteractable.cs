using UnityEngine;

public class IdealDialogueInteractable : MonoBehaviour
{
    [Header("Dialogue")]
    public IdealDialogueManager dialogueManager;
    public NPCDialogueConversation conversation;

    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;

    [Header("First-Time Interaction")]
    [SerializeField] private FirstTimeInteractable firstTimeInteractable;
    [SerializeField] private GameObject interactionIcon;

    private bool playerNearby = false;

    private void Start()
    {
        // IMPORTANT:
        // Do NOT turn the icon off here.
        //
        // The InteractionDetector on Patch already controls the
        // shared InteractionIcon for normal interactable objects.
        //
        // If this NPC also disabled the shared icon in Start(),
        // the two systems could fight over the same GameObject.
    }

    private void Update()
    {
        if (!playerNearby)
        {
            return;
        }

        if (dialogueManager == null)
        {
            return;
        }

        if (Input.GetKeyDown(interactionKey))
        {
            if (!dialogueManager.IsDialogueActive)
            {
                if (conversation == null)
                {
                    Debug.LogWarning(
                        gameObject.name +
                        ": No NPC Dialogue Conversation has been assigned."
                    );

                    return;
                }

                // Start dialogue normally.
                dialogueManager.StartDialogue(conversation);

                // Mark this NPC as interacted with after the
                // first successful dialogue interaction.
                if (firstTimeInteractable != null &&
                    !firstTimeInteractable.HasBeenInteractedWith)
                {
                    firstTimeInteractable.MarkAsInteracted();

                    if (interactionIcon != null)
                    {
                        interactionIcon.SetActive(false);
                    }
                }

                return;
            }

            // Everything below remains your existing E dialogue behaviour.
            if (dialogueManager.CurrentConversation != conversation)
            {
                return;
            }

            if (dialogueManager.IsTyping)
            {
                dialogueManager.CompleteCurrentLine();
            }
            else if (!dialogueManager.IsWaitingForChoice)
            {
                dialogueManager.ContinueDialogue();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerNearby = true;

        // NPC-specific icon behaviour.
        if (firstTimeInteractable != null &&
            !firstTimeInteractable.HasBeenInteractedWith)
        {
            if (interactionIcon != null)
            {
                interactionIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerNearby = false;

        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
    }
}