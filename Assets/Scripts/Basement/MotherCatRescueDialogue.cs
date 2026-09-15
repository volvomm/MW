using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MotherCatRescueDialogue : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [Header("Speaker Information")]
        public string speakerName;

        [TextArea(2, 5)]
        public string dialogueText;

        public Sprite speakerPortrait;
    }

    [Header("First Dialogue Lines")]
    [Tooltip("This conversation plays the FIRST time Patch talks to the Mother Cat.")]
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("Repeat Dialogue Lines")]
    [Tooltip("This conversation plays whenever Patch talks to the Mother Cat again while she is still trapped.")]
    [SerializeField] private DialogueLine[] repeatDialogueLines;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.04f;

    private int currentLineIndex;
    private Coroutine typingCoroutine;

    private bool dialogueActive;
    private bool isTyping;
    private string currentCompleteText;

    // Tracks whether the first Mother Cat conversation
    // has already been completed.
    private bool firstDialogueCompleted = false;

    // The dialogue array currently being used.
    private DialogueLine[] activeDialogueLines;

    private void Start()
    {
        // The dialogue should not be visible when the game first begins.
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!dialogueActive)
        {
            return;
        }

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            HandleEPressed();
        }
    }

    public void BeginDialogue()
    {
        // Prevent another dialogue from starting
        // while one is already active.
        if (dialogueActive)
        {
            return;
        }

        // Decide which conversation should play.
        if (!firstDialogueCompleted)
        {
            activeDialogueLines = dialogueLines;
        }
        else
        {
            activeDialogueLines = repeatDialogueLines;
        }

        // Make sure the selected conversation actually has dialogue.
        if (activeDialogueLines == null ||
            activeDialogueLines.Length == 0)
        {
            Debug.LogWarning(
                "MotherCatRescueDialogue has no dialogue lines assigned for this conversation."
            );

            return;
        }

        dialogueActive = true;
        currentLineIndex = 0;

        FreezePlayer();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        ShowCurrentLine();
    }

    private void HandleEPressed()
    {
        // If the sentence is still typing,
        // pressing E completes the sentence immediately.
        if (isTyping)
        {
            CompleteCurrentLineImmediately();
            return;
        }

        // Otherwise move to the next line.
        currentLineIndex++;

        if (currentLineIndex >= activeDialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        DialogueLine currentLine =
            activeDialogueLines[currentLineIndex];

        speakerNameText.text = currentLine.speakerName;

        currentCompleteText = currentLine.dialogueText;
        dialogueText.text = "";

        if (portraitImage != null)
        {
            portraitImage.sprite =
                currentLine.speakerPortrait;

            portraitImage.enabled =
                currentLine.speakerPortrait != null;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypeCurrentLine());
    }

    private IEnumerator TypeCurrentLine()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in currentCompleteText)
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }

        dialogueText.text = currentCompleteText;

        isTyping = false;
        typingCoroutine = null;
    }

    private void CompleteCurrentLineImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = currentCompleteText;

        isTyping = false;
    }

    private void FreezePlayer()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }

    private void UnfreezePlayer()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        dialogueActive = false;

        // If this was the FIRST conversation,
        // mark it as completed.
        if (!firstDialogueCompleted)
        {
            firstDialogueCompleted = true;

            // Keep your existing story progression behaviour.
            StoryProgress.MotherCatRescueDialogueFinished = true;
        }

        dialogueText.text = "";

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        UnfreezePlayer();
    }
}