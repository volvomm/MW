using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MotherCatRescueDialogue : MonoBehaviour, IInteractable
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

    private void Start()
    {
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

    // =========================================================
    // IINTERACTABLE
    // =========================================================

    public bool CanInteract()
    {
        // Patch can only use the reminder dialogue after
        // the original RevealMotherCat conversation has finished.
        return StoryProgress.MotherCatRescueDialogueFinished;
    }

    public void Interact()
    {
        if (dialogueActive)
        {
            return;
        }

        BeginDialogue();
    }

    // =========================================================
    // START DIALOGUE
    // =========================================================

    public void BeginDialogue()
    {
        if (dialogueActive)
        {
            return;
        }

        // This script now only handles the repeat/reminder dialogue.
        if (repeatDialogueLines == null ||
            repeatDialogueLines.Length == 0)
        {
            Debug.LogWarning(
                "MotherCatRescueDialogue has no repeat dialogue lines assigned."
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

    // =========================================================
    // E INPUT DURING DIALOGUE
    // =========================================================

    private void HandleEPressed()
    {
        // If the line is still typing,
        // pressing E completes it immediately.
        if (isTyping)
        {
            CompleteCurrentLineImmediately();
            return;
        }

        // Otherwise go to the next line.
        currentLineIndex++;

        if (currentLineIndex >= repeatDialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    // =========================================================
    // SHOW LINE
    // =========================================================

    private void ShowCurrentLine()
    {
        DialogueLine currentLine =
            repeatDialogueLines[currentLineIndex];

        if (speakerNameText != null)
        {
            speakerNameText.text = currentLine.speakerName;
        }

        currentCompleteText = currentLine.dialogueText;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

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

    // =========================================================
    // TYPEWRITER
    // =========================================================

    private IEnumerator TypeCurrentLine()
    {
        isTyping = true;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        foreach (char letter in currentCompleteText)
        {
            if (dialogueText != null)
            {
                dialogueText.text += letter;
            }

            yield return new WaitForSeconds(
                typingSpeed
            );
        }

        if (dialogueText != null)
        {
            dialogueText.text = currentCompleteText;
        }

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

        if (dialogueText != null)
        {
            dialogueText.text = currentCompleteText;
        }

        isTyping = false;
    }

    // =========================================================
    // PLAYER MOVEMENT
    // =========================================================

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

    // =========================================================
    // END DIALOGUE
    // =========================================================

    private void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        dialogueActive = false;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        UnfreezePlayer();
    }
}