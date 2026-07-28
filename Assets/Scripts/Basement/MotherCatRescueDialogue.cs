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

    [Header("Dialogue Lines")]
    [SerializeField] private DialogueLine[] dialogueLines;

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
        // Prevent this dialogue from being started again while already active.
        if (dialogueActive)
        {
            return;
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning(
                "MotherCatRescueDialogue has no dialogue lines assigned."
            );

            return;
        }

        dialogueActive = true;
        currentLineIndex = 0;

        FreezePlayer();

        dialoguePanel.SetActive(true);

        ShowCurrentLine();
    }

    private void HandleEPressed()
    {
        // If the sentence is still typing, the first E press finishes it.
        if (isTyping)
        {
            CompleteCurrentLineImmediately();
            return;
        }

        // Otherwise, move to the next dialogue line.
        currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        DialogueLine currentLine = dialogueLines[currentLineIndex];

        speakerNameText.text = currentLine.speakerName;

        currentCompleteText = currentLine.dialogueText;
        dialogueText.text = "";

        if (portraitImage != null)
        {
            portraitImage.sprite = currentLine.speakerPortrait;

            portraitImage.enabled =
                currentLine.speakerPortrait != null;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeCurrentLine());
    }

    private IEnumerator TypeCurrentLine()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in currentCompleteText)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
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
            playerRigidbody.linearVelocity = Vector2.zero;
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
            playerRigidbody.linearVelocity = Vector2.zero;
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

        dialogueText.text = "";

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        UnfreezePlayer();
    }
}