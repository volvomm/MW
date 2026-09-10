using UnityEngine;

public class IdealDialogueInteractable : MonoBehaviour
{
    [Header("Dialogue")]
    public IdealDialogueManager dialogueManager;
    public NPCDialogueConversation conversation;

    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;

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

                dialogueManager.StartDialogue(conversation);
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
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}