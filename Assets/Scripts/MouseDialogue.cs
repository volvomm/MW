using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MouseDialogue : MonoBehaviour
{
    public enum Speaker
    {
        Cat,
        Mouse
    }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker;
        [TextArea(2, 4)]
        public string text;
    }

    [Header("Dialogue Lines")]
    public DialogueLine[] dialogueLines;

    [Header("UI References")]
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image portraitImage;

    [Header("Portraits")]
    public Sprite catPortrait;
    public Sprite mousePortrait;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    [Header("Mouse Movement")]
    public MouseMovement mouseMovement;

    private int currentLine = 0;
    private bool playerInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isDialogueActive)
            {
                StartDialogue();
            }
            else
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
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        currentLine = 0;
        dialogueUI.SetActive(true);
        ShowLine();
    }

    void NextLine()
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

    void ShowLine()
    {
        DialogueLine line = dialogueLines[currentLine];

        if (line.speaker == Speaker.Cat)
        {
            nameText.text = "Patch";
            portraitImage.sprite = catPortrait;
        }
        else if (line.speaker == Speaker.Mouse)
        {
            nameText.text = "Mouse";
            portraitImage.sprite = mousePortrait;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine(line.text));
    }

    IEnumerator TypeLine(string line)
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

    void EndDialogue()
    {
        dialogueUI.SetActive(false);
        isDialogueActive = false;

        if (mouseMovement != null)
        {
            mouseMovement.StartMoving();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}