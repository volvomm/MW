using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChaseIntroTrigger : MonoBehaviour
{
    [Header("Player")]
    public PlayerMovement playerMovement;

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;

    [Header("Typing")]
    public float typingSpeed = 0.04f;

    private string[] speakerNames =
    {
        "",
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
        if (!other.CompareTag("Player"))
            return;

        if (sequenceRunning)
            return;

        if (ChaseSequenceManager.Instance == null)
            return;

        // Do not start until the reunion has finished.
        if (!ChaseSequenceManager.Instance.reunionFinished)
            return;

        // Do not play this sequence twice.
        if (ChaseSequenceManager.Instance.chaseIntroFinished)
            return;

        StartCoroutine(BeginSequence());
    }

    private IEnumerator BeginSequence()
    {
        sequenceRunning = true;

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

        typingCoroutine = StartCoroutine(TypeCurrentLine());
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