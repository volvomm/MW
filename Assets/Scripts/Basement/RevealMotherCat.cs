using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RevealMotherCat : MonoBehaviour, IInteractable
{
    public enum Speaker
    {
        Patch,
        MotherCat
    }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker;

        [TextArea(2, 4)]
        public string text;
    }

    [Header("Objects")]
    public GameObject coveredCage;
    public GameObject cage;
    public GameObject motherCat;

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image portraitImage;

    [Header("Portraits")]
    public Sprite patchPortrait;
    public Sprite motherCatPortrait;

    [Header("Dialogue Lines")]
    public DialogueLine[] dialogueLines;

    [Header("Dialogue Settings")]
    public float typingSpeed = 0.04f;

    private bool alreadyRevealed = false;
    private bool dialogueActive = false;
    private bool isTyping = false;
    private bool canUseEForDialogue = false;
    private int currentLine = 0;
    private Coroutine typingCoroutine;

    public bool CanInteract()
    {
        return !alreadyRevealed;
    }

    public void Interact()
    {
        if (alreadyRevealed)
            return;

        alreadyRevealed = true;

        coveredCage.SetActive(false);
        cage.SetActive(true);
        motherCat.SetActive(true);

        StartDialogue();
    }

    void Update()
    {
        if (!dialogueActive)
            return;

        if (!canUseEForDialogue)
        {
            if (Input.GetKeyUp(KeyCode.E))
                canUseEForDialogue = true;

            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = dialogueLines[currentLine].text;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    private void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            EndDialogue();
            return;
        }

        dialogueActive = true;
        canUseEForDialogue = false;
        currentLine = 0;

        dialoguePanel.SetActive(true);
        ShowLine();
    }

    private void NextLine()
    {
        currentLine++;

        if (currentLine < dialogueLines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowLine()
    {
        DialogueLine line = dialogueLines[currentLine];

        if (line.speaker == Speaker.Patch)
        {
            nameText.text = "Patch";

            if (portraitImage != null && patchPortrait != null)
                portraitImage.sprite = patchPortrait;
        }
        else if (line.speaker == Speaker.MotherCat)
        {
            nameText.text = "Mother Cat";

            if (portraitImage != null && motherCatPortrait != null)
                portraitImage.sprite = motherCatPortrait;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(line.text));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        currentLine = 0;
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
    }
}