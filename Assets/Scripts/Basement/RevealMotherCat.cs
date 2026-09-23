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

    [Header("Player Movement Lock")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Animator playerAnimator;

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
        {
            return;
        }

        alreadyRevealed = true;

        coveredCage.SetActive(false);
        cage.SetActive(true);
        motherCat.SetActive(true);

        StartDialogue();
    }

    private void Update()
    {
        if (!dialogueActive)
        {
            return;
        }

        if (!canUseEForDialogue)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                canUseEForDialogue = true;
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                    typingCoroutine = null;
                }

                dialogueText.text =
                    dialogueLines[currentLine].text;

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
        if (dialogueLines == null ||
            dialogueLines.Length == 0)
        {
            EndDialogue();
            return;
        }

        dialogueActive = true;
        canUseEForDialogue = false;
        currentLine = 0;

        // Freeze Patch for the entire conversation.
        FreezePlayer();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

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
        DialogueLine line =
            dialogueLines[currentLine];

        if (line.speaker == Speaker.Patch)
        {
            nameText.text = "Patch";

            if (portraitImage != null &&
                patchPortrait != null)
            {
                portraitImage.sprite =
                    patchPortrait;
            }
        }
        else if (line.speaker == Speaker.MotherCat)
        {
            nameText.text = "Mother Cat";

            if (portraitImage != null &&
                motherCatPortrait != null)
            {
                portraitImage.sprite =
                    motherCatPortrait;
            }
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypeLine(line.text));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;

        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }

        dialogueText.text = line;

        isTyping = false;
        typingCoroutine = null;
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueActive = false;
        isTyping = false;
        canUseEForDialogue = false;
        currentLine = 0;

        dialogueText.text = "";

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Give Patch control back only after the conversation ends.
        UnfreezePlayer();

        StoryProgress.MotherCatRescueDialogueFinished = true;

        // Mother Cat has now told Patch to talk to
        // the kittens for the puzzle-box number hints.
        StoryProgress.KittenNumberHintsUnlocked = true;
    }

    private void FreezePlayer()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
        }

        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
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

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
        }

        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
            playerMovement.enabled = true;
        }
    }
}