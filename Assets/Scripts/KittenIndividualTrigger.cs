using UnityEngine;

public class KittenIndividualTrigger : MonoBehaviour
{
    public KittenGroupDialogue individualDialogue;

    private bool playerInRange;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            KittenGroupDialogueManager.Instance.StartDialogue(individualDialogue);
        }
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