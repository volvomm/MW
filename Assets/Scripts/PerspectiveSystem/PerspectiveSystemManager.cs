using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IdealDialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;

    [Header("Choice UI")]
    public GameObject choicePanel;
    public Button[] choiceButtons;

    [Header("Dialogue Nodes")]
    public DialogueNode[] dialogueNodes;

    [Header("Dialogue Settings")]
    public int startingNodeIndex = 0;

    [Tooltip("Seconds between each character appearing.")]
    public float typingSpeed = 0.04f;

    private int currentNodeIndex = -1;
    private bool dialogueActive = false;
    private bool waitingForChoice = false;

    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string currentFullText = "";

    public bool IsDialogueActive => dialogueActive;
    public bool IsWaitingForChoice => waitingForChoice;
    public bool IsTyping => isTyping;

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
    }

    [ContextMenu("TEST - Start Dialogue")]
    public void StartDialogue()
    {
        if (dialogueNodes == null || dialogueNodes.Length == 0)
        {
            Debug.LogWarning("IdealDialogueManager: No dialogue nodes have been added.");
            return;
        }

        dialogueActive = true;
        waitingForChoice = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        ShowNode(startingNodeIndex);
    }

    public void ShowNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= dialogueNodes.Length)
        {
            EndDialogue();
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;

        currentNodeIndex = nodeIndex;

        DialogueNode node = dialogueNodes[currentNodeIndex];

        if (speakerNameText != null)
        {
            speakerNameText.text = node.speakerName;
        }

        if (portraitImage != null)
        {
            if (node.portrait != null)
            {
                portraitImage.sprite = node.portrait;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.enabled = false;
            }
        }

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        waitingForChoice = false;

        currentFullText = node.dialogueText;

        typingCoroutine = StartCoroutine(TypeDialogue(currentFullText));
    }

    private IEnumerator TypeDialogue(string textToType)
    {
        isTyping = true;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        foreach (char character in textToType)
        {
            dialogueText.text += character;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        DialogueNode node = dialogueNodes[currentNodeIndex];

        if (node.hasChoices)
        {
            ShowChoices(node);
        }
    }

    public void CompleteCurrentLine()
    {
        if (!isTyping)
        {
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = currentFullText;
        }

        isTyping = false;

        DialogueNode node = dialogueNodes[currentNodeIndex];

        if (node.hasChoices)
        {
            ShowChoices(node);
        }
    }

    private void ShowChoices(DialogueNode node)
    {
        waitingForChoice = true;

        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
        }

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(false);
            choiceButtons[i].onClick.RemoveAllListeners();
        }

        if (node.choices == null)
        {
            return;
        }

        for (int i = 0; i < node.choices.Length; i++)
        {
            if (i >= choiceButtons.Length)
            {
                break;
            }

            int choiceIndex = i;

            Button button = choiceButtons[i];

            button.gameObject.SetActive(true);

            TextMeshProUGUI buttonText =
                button.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                buttonText.text = node.choices[i].choiceText;
            }

            button.onClick.AddListener(() =>
            {
                SelectChoice(choiceIndex);
            });
        }
    }

    public void SelectChoice(int choiceIndex)
    {
        if (!waitingForChoice)
        {
            return;
        }

        DialogueNode currentNode = dialogueNodes[currentNodeIndex];

        if (currentNode.choices == null)
        {
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= currentNode.choices.Length)
        {
            return;
        }

        DialogueChoice selectedChoice =
            currentNode.choices[choiceIndex];

        waitingForChoice = false;

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        ShowNode(selectedChoice.nextNodeIndex);
    }

    [ContextMenu("TEST - Continue Dialogue")]
    public void ContinueDialogue()
    {
        if (!dialogueActive)
        {
            return;
        }

        if (isTyping)
        {
            CompleteCurrentLine();
            return;
        }

        if (waitingForChoice)
        {
            return;
        }

        DialogueNode currentNode = dialogueNodes[currentNodeIndex];

        if (currentNode.nextNodeIndex == -1)
        {
            EndDialogue();
        }
        else
        {
            ShowNode(currentNode.nextNodeIndex);
        }
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        dialogueActive = false;
        waitingForChoice = false;
        currentNodeIndex = -1;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
    }
}