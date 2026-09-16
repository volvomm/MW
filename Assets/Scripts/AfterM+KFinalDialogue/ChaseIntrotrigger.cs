using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChaseIntroTrigger : MonoBehaviour
{
    [Header("Player")]
    public PlayerMovement playerMovement;
    public InteractionDetector interactionDetector;

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;

    [Header("Dialogue Portraits")]
    public Sprite patchPortrait;
    public NPCPerspectiveSetup devilDogPerspective;

    [Header("Typing")]
    public float typingSpeed = 0.04f;

    private string[] speakerNames =
{
    "Devil Dog",
    "Devil Dog",
    "Patch"
};

    private string[] dialogueLines =
    {
        "*Thud!*",
        "You vile gremlin! I'll kill you!",
        "Oh no! I need to escape before he breaks through the door!"
    };

    private int currentLine = 0;

    private bool sequenceRunning = false;
    private bool lineTyping = false;
    private bool lineFullyDisplayed = false;

    private Coroutine typingCoroutine;

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryStartChase(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryStartChase(other);
    }

    private void TryStartChase(Collider2D other)
    {
        // Only react to Patch.
        if (!other.CompareTag("Player"))
            return;

        // Don't start it again while it is already running.
        if (sequenceRunning)
            return;

        // Make sure the chase manager exists.
        if (ChaseSequenceManager.Instance == null)
            return;

        // Reunion must be finished first.
        if (!ChaseSequenceManager.Instance.reunionFinished)
            return;

        // Don't repeat the chase intro after it has finished.
        if (ChaseSequenceManager.Instance.chaseIntroFinished)
            return;

        Debug.Log("PATCH IS INSIDE CHASE INTRO TRIGGER - Starting chase dialogue.");

        StartCoroutine(BeginSequence());
    }

    private IEnumerator BeginSequence()
    {
        sequenceRunning = true;

        // Prevent E from activating doors/items while the chase
        // dialogue is using E.
        if (interactionDetector != null)
        {
            interactionDetector.interactionsLocked = true;
        }

        // Freeze Patch.
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Stop any leftover Rigidbody movement.
        Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        dialoguePanel.SetActive(true);

        currentLine = 0;

        StartTypingCurrentLine();

        yield return null;
    }

    private void Update()
    {
        if (!sequenceRunning)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        // If text is still typing, first E press reveals all of it.
        if (lineTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            dialogueText.text = dialogueLines[currentLine];

            lineTyping = false;
            lineFullyDisplayed = true;

            return;
        }

        // If the whole line is showing, move to next line.
        if (lineFullyDisplayed)
        {
            currentLine++;

            if (currentLine >= dialogueLines.Length)
            {
                FinishSequence();
                return;
            }

            StartTypingCurrentLine();
        }
    }

    private void StartTypingCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        UpdatePortrait();

        typingCoroutine = StartCoroutine(TypeCurrentLine());
    }

    private void UpdatePortrait()
    {
        if (portraitImage == null)
            return;

        // Lines 0 and 1 belong to the Devil Dog sequence.
        // Use the Devil Dog portrait that matches
        // Patch's current perspective/mood.
        if (currentLine == 0 || currentLine == 1)
        {
            if (devilDogPerspective != null)
            {
                Sprite dogPortrait =
                    devilDogPerspective.GetCurrentDialoguePortrait();

                if (dogPortrait != null)
                {
                    portraitImage.sprite = dogPortrait;
                    portraitImage.gameObject.SetActive(true);
                    return;
                }
            }

            portraitImage.sprite = null;
            portraitImage.gameObject.SetActive(false);
            return;
        }

        // Line 2 = Patch
        if (currentLine == 2)
        {
            portraitImage.sprite = patchPortrait;
            portraitImage.gameObject.SetActive(patchPortrait != null);
        }
    }

    private IEnumerator TypeCurrentLine()
    {
        lineTyping = true;
        lineFullyDisplayed = false;

        dialogueText.text = "";

        if (speakerNameText != null)
        {
            speakerNameText.text = speakerNames[currentLine];
        }

        foreach (char letter in dialogueLines[currentLine])
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        lineTyping = false;
        lineFullyDisplayed = true;
    }

    private void FinishSequence()
    {
        sequenceRunning = false;

        dialoguePanel.SetActive(false);

        // World interactions can use E again.
        if (interactionDetector != null)
        {
            interactionDetector.interactionsLocked = false;
        }

        if (ChaseSequenceManager.Instance != null)
        {
            ChaseSequenceManager.Instance.MarkChaseIntroFinished();
        }

        // Give control back to Patch.
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Trigger should never be needed again.
        gameObject.SetActive(false);
    }
}