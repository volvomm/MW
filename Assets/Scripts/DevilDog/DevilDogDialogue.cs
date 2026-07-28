using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevilDogDialogue : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public Sprite speakerPortrait;

        [TextArea(3, 6)]
        public string sentence;
    }

    [Header("Dialogue Lines")]
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private GameObject closeButton;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private Behaviour playerMovementScript;

    [Header("Interaction")]
    [SerializeField] private GameObject interactionIcon;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Repeat Settings")]
    [SerializeField] private bool playOnlyOnce;

    private bool playerIsInRange;
    private bool dialogueIsActive;
    private bool isTyping;
    private bool dialogueCompleted;

    private int currentLineIndex;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(interactionKey))
        {
            return;
        }

        if (dialogueIsActive)
        {
            HandleDialogueInput();
            return;
        }

        if (playOnlyOnce && dialogueCompleted)
        {
            return;
        }

        if (playerIsInRange)
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("No Devil Dog dialogue lines assigned.", this);
            return;
        }

        dialogueIsActive = true;
        currentLineIndex = 0;

        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        SetPlayerMovement(false);
        DisplayCurrentLine();
    }

    private void HandleDialogueInput()
    {
        if (isTyping)
        {
            CompleteCurrentSentence();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        DialogueLine line = dialogueLines[currentLineIndex];

        if (speakerNameText != null)
        {
            speakerNameText.text = line.speakerName;
        }

        if (speakerPortraitImage != null)
        {
            speakerPortraitImage.sprite = line.speakerPortrait;
            speakerPortraitImage.enabled = line.speakerPortrait != null;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeSentence(line.sentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;

        dialogueText.text = sentence;
        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i <= sentence.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void CompleteCurrentSentence()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        DialogueLine line = dialogueLines[currentLineIndex];

        dialogueText.text = line.sentence;
        dialogueText.maxVisibleCharacters = line.sentence.Length;

        isTyping = false;
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueIsActive = false;
        isTyping = false;
        dialogueCompleted = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.SetActive(true);
        }

        SetPlayerMovement(true);

        if (interactionIcon != null &&
            playerIsInRange &&
            !(playOnlyOnce && dialogueCompleted))
        {
            interactionIcon.SetActive(true);
        }
    }

    private void SetPlayerMovement(bool canMove)
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = canMove;
        }

        if (!canMove &&
            player != null &&
            player.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerIsInRange = true;

        if (!dialogueIsActive &&
            !(playOnlyOnce && dialogueCompleted) &&
            interactionIcon != null)
        {
            interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerIsInRange = false;

        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
    }
}