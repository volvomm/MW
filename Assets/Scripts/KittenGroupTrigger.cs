using UnityEngine;

public class KittenGroupTrigger : MonoBehaviour
{
    public KittenGroupDialogue mainGroupDialogue;

    public GameObject[] individualKittenDialogueTriggers;

    private bool playerInRange;
    private bool mainDialogueFinished;

    private void Start()
    {
        foreach (GameObject trigger in individualKittenDialogueTriggers)
        {
            trigger.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && !mainDialogueFinished && Input.GetKeyDown(KeyCode.E))
        {
            KittenGroupDialogueManager.Instance.StartDialogue(mainGroupDialogue, FinishMainDialogue);
        }
    }

    private void FinishMainDialogue()
    {
        mainDialogueFinished = true;

        foreach (GameObject trigger in individualKittenDialogueTriggers)
        {
            trigger.SetActive(true);
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