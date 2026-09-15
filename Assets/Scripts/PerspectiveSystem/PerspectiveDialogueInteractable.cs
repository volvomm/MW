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

                // Start the dialogue normally.
                dialogueManager.StartDialogue(conversation);

                // This is now considered Patch's first interaction
                // with this NPC.
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
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            // Show ! only if Patch has never interacted
            // with this NPC before.
            if (firstTimeInteractable != null &&
                !firstTimeInteractable.HasBeenInteractedWith)
            {
                if (interactionIcon != null)
                {
                    interactionIcon.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (interactionIcon != null)
            {
                interactionIcon.SetActive(false);
            }
        }
    }
}