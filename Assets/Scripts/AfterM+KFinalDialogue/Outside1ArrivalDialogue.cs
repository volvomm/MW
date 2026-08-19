using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Outside1ArrivalDialogue : MonoBehaviour
{
    [Header("Player")]
    public PlayerMovement playerMovement;

    [Header("Dialogue")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;

    [Header("Typing")]
    public float typingSpeed = 0.04f;

    private string line = "Quickly, I need to keep going!";

    private bool sequenceStarted = false;
    private bool typing = false;
    private bool finishedTyping = false;

    private Coroutine typingCoroutine;

    private void Update()
    {
        if (sequenceStarted)
        {
            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                HandleE();
            }

            return;
        }

        if (ChaseSequenceManager.Instance == null)
            return;

        if (!ChaseSequenceManager.Instance.reachedOutside)
            return;

        BeginDialogue();
    }

    private void BeginDialogue()
    {
        sequenceStarted = true;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;

            Rigidbody2D rb =
                playerMovement.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        dialoguePanel.SetActive(true);

        if (speakerNameText != null)
        {
            speakerNameText.text = "Patch";
        }

        typingCoroutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        typing = true;
        finishedTyping = false;

        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        typing = false;
        finishedTyping = true;
    }

    private void HandleE()
    {
        if (typing)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            dialogueText.text = line;

            typing = false;
            finishedTyping = true;

            return;
        }

        if (finishedTyping)
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        enabled = false;
    }
}