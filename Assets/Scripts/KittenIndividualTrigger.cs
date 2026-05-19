using UnityEngine;

public class KittenIndividualTrigger : MonoBehaviour
{
    public KittenGroupDialogue individualDialogue;

    private bool playerInRange;
    private bool waitingForKeyRelease;

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        if (waitingForKeyRelease)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                waitingForKeyRelease = false;
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (KittenGroupDialogueManager.Instance.IsDialogueActive())
            {
                return;
            }

            waitingForKeyRelease = true;
            KittenGroupDialogueManager.Instance.StartDialogue(individualDialogue, OnDialogueFinished);
        }
    }

    private void OnDialogueFinished()
    {
        waitingForKeyRelease = true;
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
            waitingForKeyRelease = false;
        }
    }
}