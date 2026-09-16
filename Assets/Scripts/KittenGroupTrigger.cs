using UnityEngine;

public class KittenGroupTrigger : MonoBehaviour
{
    [Header("Perspective Dialogue System")]
    public IdealDialogueManager dialogueManager;
    public NPCDialogueConversation conversation;

    [Header("Individual Kitten Dialogues")]
    public GameObject[] individualKittenDialogueTriggers;

    [Header("Interaction")]
    public KeyCode interactionKey = KeyCode.E;

    private bool playerInRange;
    private bool mainDialogueFinished;

    private void Start()
    {
        foreach (GameObject trigger in individualKittenDialogueTriggers)
        {
            if (trigger != null)
            {
                trigger.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (mainDialogueFinished)
            return;

        if (dialogueManager == null || conversation == null)
            return;

        if (Input.GetKeyDown(interactionKey))
        {
            // Start the conversation.
            if (!dialogueManager.IsDialogueActive)
            {
                dialogueManager.OnDialogueFinished = FinishMainDialogue;
                dialogueManager.StartDialogue(conversation);
                return;
            }

            // Only control dialogue if THIS is the active conversation.
            if (dialogueManager.CurrentConversation != conversation)
                return;

            // E completes the typewriter first.
            if (dialogueManager.IsTyping)
            {
                dialogueManager.CompleteCurrentLine();
            }
            // Otherwise E advances, unless we're choosing an answer.
            else if (!dialogueManager.IsWaitingForChoice)
            {
                dialogueManager.ContinueDialogue();
            }
        }
    }

    private void FinishMainDialogue()
    {
        mainDialogueFinished = true;

        foreach (GameObject trigger in individualKittenDialogueTriggers)
        {
            if (trigger != null)
            {
                trigger.SetActive(true);
            }
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}