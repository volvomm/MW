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

    [Header("First Devil Dog Dialogue")]
    [SerializeField] private DialogueLine[] firstDialogueLines;

    [Header("Second Devil Dog Lure Dialogue")]
    [SerializeField] private DialogueLine[] lureDialogueLines;

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

    [Header("Devil Dog Walk Away")]
    [SerializeField] private DevilDogWalkOff devilDogWalkOff;

    private bool playerIsInRange;
    private bool dialogueIsActive;
    private bool isTyping;

    private bool firstDialogueCompleted;
    private bool lureDialogueCompleted;

    private bool playingLureDialogue;

    private int currentLineIndex;

    private DialogueLine[] currentDialogueLines;

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

        if (!playerIsInRange)
        {
            return;
        }

        // FIRST INTERACTION:
        // Patch has not spoken to the Devil Dog yet.
        if (!firstDialogueCompleted)
        {
            StartFirstDialogue();
            return;
        }

        // SECOND INTERACTION:
        // Only becomes available after BOTH story requirements are complete.
        if (!lureDialogueCompleted &&
            StoryProgress.MotherCatRescueDialogueFinished &&
            StoryProgress.RecorderPuzzleFinished)
        {
            StartLureDialogue();
            return;
        }

        // If neither condition is available,
        // pressing E does nothing for now.
    }

    private void StartFirstDialogue()
    {
        if (firstDialogueLines == null ||
            firstDialogueLines.Length == 0)
        {
            Debug.LogWarning(
                "No first Devil Dog dialogue lines assigned.",
                this
            );

            return;
        }

        playingLureDialogue = false;
        currentDialogueLines = firstDialogueLines;

        StartDialogue();
    }

    private void StartLureDialogue()
    {
        if (lureDialogueLines == null ||
            lureDialogueLines.Length == 0)
        {
            Debug.LogWarning(
                "No Devil Dog lure dialogue lines assigned.",
                this
            );

            return;
        }

        playingLureDialogue = true;
        currentDialogueLines = lureDialogueLines;

        StartDialogue();
    }

    private void StartDialogue()
    {
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

        if (currentLineIndex >= currentDialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        DialogueLine line =
            currentDialogueLines[currentLineIndex];

        if (speakerNameText != null)
        {
            speakerNameText.text = line.speakerName;
        }

        if (speakerPortraitImage != null)
        {
            speakerPortraitImage.sprite = line.speakerPortrait;

            speakerPortraitImage.enabled =
                line.speakerPortrait != null;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypeSentence(line.sentence));
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

        DialogueLine line =
            currentDialogueLines[currentLineIndex];

        dialogueText.text = line.sentence;

        //dialogueText.maxVisibleCharacters =
        //line.sentence.Length;

        dialogueText.maxVisibleCharacters = 100;

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

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.SetActive(true);
        }

        SetPlayerMovement(true);

        // FIRST DEVIL DOG CONVERSATION FINISHED
        if (!playingLureDialogue)
        {
            firstDialogueCompleted = true;

            StoryProgress.HasTalkedToDevilDog = true;

            if (interactionIcon != null &&
                playerIsInRange &&
                StoryProgress.MotherCatRescueDialogueFinished &&
                StoryProgress.RecorderPuzzleFinished)
            {
                interactionIcon.SetActive(true);
            }

            return;
        }

        // SECOND / LURE CONVERSATION FINISHED
        lureDialogueCompleted = true;

        StoryProgress.DevilDogLureDialogueFinished = true;

        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }

        if (devilDogWalkOff != null)
        {
            devilDogWalkOff.StartWalkingAway();
        }
        else
        {
            Debug.LogWarning(
                "DevilDogWalkOff has not been assigned.",
                this
            );
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

        // First interaction is always available.
        if (!firstDialogueCompleted)
        {
            if (interactionIcon != null)
            {
                interactionIcon.SetActive(true);
            }

            return;
        }

        // After the first dialogue, only show E when
        // the second conversation is actually available.
        if (!lureDialogueCompleted &&
            StoryProgress.MotherCatRescueDialogueFinished &&
            StoryProgress.RecorderPuzzleFinished)
        {
            if (interactionIcon != null)
            {
                interactionIcon.SetActive(true);
            }

            return;
        }

        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
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