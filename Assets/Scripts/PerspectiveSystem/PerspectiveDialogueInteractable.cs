using UnityEngine;

public class IdealDialogueInteractable : MonoBehaviour
{
    [Header("Dialogue")]
    public IdealDialogueManager dialogueManager;

    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;

    private bool playerNearby = false;

    private void Update()
    {
        if (!playerNearby)
        {
            return;
        }

        if (Input.GetKeyDown(interactionKey))
        {
            if (!dialogueManager.IsDialogueActive)
            {
                dialogueManager.StartDialogue();
            }
            else if (dialogueManager.IsTyping)
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