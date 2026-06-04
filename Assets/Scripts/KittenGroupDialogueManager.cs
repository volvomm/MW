using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KittenGroupDialogueManager : MonoBehaviour
{
    public static KittenGroupDialogueManager Instance;

    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }

    public MonoBehaviour playerMovementScript;

    private KittenGroupDialogue currentDialogue;
    private int currentLine;
    private bool isTyping;
    private bool dialogueActive;
    private Coroutine typingCoroutine;

    private System.Action onDialogueFinished;

    private void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(KittenGroupDialogue dialogue, System.Action finishedCallback = null)
    {
        if (dialogueActive) return;

        currentDialogue = dialogue;
        currentLine = 0;
        onDialogueFinished = finishedCallback;
        dialogueActive = true;

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        dialoguePanel.SetActive(true);
        ShowLine();
    }

    public void ForceEndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        dialoguePanel.SetActive(false);
        dialogueActive = false;

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        onDialogueFinished?.Invoke();
        onDialogueFinished = null;
    }

    private void Update()
    {
        if (dialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            NextLine();
        }
    }

    private void ShowLine()
    {
        if (currentDialogue == null || currentDialogue.dialogueLines == null || currentDialogue.dialogueLines.Length == 0)
        {
            EndDialogue();
            return;
        }

        if (currentLine < 0 || currentLine >= currentDialogue.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        KittenGroupDialogue.DialogueLine line = currentDialogue.dialogueLines[currentLine];

        nameText.text = line.speakerName;

        if (line.speakerPortrait != null)
        {
            portraitImage.enabled = true;
            portraitImage.sprite = line.speakerPortrait;
        }
        else
        {
            portraitImage.enabled = false;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine(line.dialogueText));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        if (string.IsNullOrEmpty(text))
        {
            isTyping = false;
            yield break;
        }

        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(currentDialogue.typingSpeed);
        }

        isTyping = false;
    }

    public void NextLine()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentDialogue.dialogueLines[currentLine].dialogueText;
            isTyping = false;
            return;
        }

        currentLine++;

        if (currentLine >= currentDialogue.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueActive = false;

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        if (CatMoodManager.Instance != null)
        {
            CatMoodManager.Instance.SetMood("Frightened");
        }

        onDialogueFinished?.Invoke();
        onDialogueFinished = null;
    }
}