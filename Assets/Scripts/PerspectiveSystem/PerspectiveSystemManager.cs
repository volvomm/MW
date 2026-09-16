using System.Collections;
using System.Collections.Generic;
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

    [Header("Mood System")]
    public MoodManager moodManager;

    [Header("Player")]
    public PlayerMovement playerMovement;

    [Header("Current Conversation")]
    public DialogueNode[] dialogueNodes;

    private int startingNodeIndex = 0;
    private NPCDialogueConversation currentConversation;

    [Tooltip("Seconds between each character appearing.")]
    public float typingSpeed = 0.04f;

    private int currentNodeIndex = -1;
    private bool dialogueActive = false;
    private bool waitingForChoice = false;

    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string currentFullText = "";

    private bool perceptionEventRunning = false;

    public System.Action OnDialogueFinished;

    private HashSet<int> completedPerceptionEvents =
        new HashSet<int>();

    public bool IsDialogueActive => dialogueActive;
    public bool IsWaitingForChoice => waitingForChoice;
    public bool IsTyping => isTyping;

    public NPCDialogueConversation CurrentConversation =>
    currentConversation;

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

    public void StartDialogue(NPCDialogueConversation conversation)
    {
        if (conversation == null)
        {
            Debug.LogWarning(
                "IdealDialogueManager: No NPC Dialogue Conversation was provided."
            );
            return;
        }

        currentConversation = conversation;

        dialogueNodes = conversation.dialogueNodes;
        startingNodeIndex = conversation.startingNodeIndex;

        if (conversation.perspectiveSetup != null)
        {
            conversation.perspectiveSetup.ApplyCurrentMoodVisual();
        }

        StartDialogue();
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

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        perceptionEventRunning = false;
        completedPerceptionEvents.Clear();

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

        if (node.requireMood && moodManager != null)
        {
            if (moodManager.currentMood != node.requiredMood)
            {
                if (node.failedMoodNodeIndex == -1)
                {
                    EndDialogue();
                    return;
                }

                ShowNode(node.failedMoodNodeIndex);
                return;
            }
        }

        if (node.changeMood && moodManager != null)
        {
            moodManager.SetMood(node.newMood);
        }

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

        // Turn the normal dialogue panel back on.
        // If this is a choice-only node, it will be hidden again below.
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // CHOICE-ONLY NODE
        // This node does not display normal dialogue.
        // It immediately opens Patch's choice UI.
        if (node.choiceOnlyNode && node.hasChoices)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            ShowChoices(node);
            return;
        }

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

        if (choiceIndex < 0 ||
            choiceIndex >= currentNode.choices.Length)
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

        if (selectedChoice.changeMood &&
            moodManager != null)
        {
            moodManager.SetMood(selectedChoice.newMood);
        }

        if (selectedChoice.triggerPerceptionChange)
        {
            StartCoroutine(
                RunChoicePerceptionEvent(selectedChoice)
            );

            return;
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

        if (perceptionEventRunning)
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

        // If this line contains choices, wait until the player
        // presses E after the line has finished before showing them.
        if (currentNode.hasChoices)
        {
            ShowChoices(currentNode);
            return;
        }

        // Perception transformation event
        if (currentNode.triggerPerceptionEvent &&
            !completedPerceptionEvents.Contains(currentNodeIndex))
        {
            StartCoroutine(RunPerceptionEvent(currentNode));
            return;
        }

        // NPC exit event
        if (currentNode.triggerNPCExit)
        {
            StartCoroutine(RunNPCExitEvent(currentNode));
            return;
        }

        if (currentNode.nextNodeIndex == -1)
        {
            EndDialogue();
        }
        else
        {
            ShowNode(currentNode.nextNodeIndex);
        }
    }

    private IEnumerator RunPerceptionEvent(DialogueNode node)
    {
        perceptionEventRunning = true;

        completedPerceptionEvents.Add(currentNodeIndex);

        int nextNode = node.nextNodeIndex;

        // Hide the dialogue during the perception pause.
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        // IMPORTANT:
        // Wait first.
        // Patch stays Hungry during this entire delay.
        yield return new WaitForSeconds(node.perceptionDelay);

        // AFTER the delay, change Patch's mood.
        if (node.changeMoodDuringPerception && moodManager != null)
        {
            moodManager.SetMood(node.perceptionMood);
        }

        // AFTER the delay, perform the visual perception change.
        if (node.perceptionChangeType == PerceptionChangeType.SpriteSwap)
        {
            if (node.perceptionTarget != null &&
                node.replacementSprite != null)
            {
                node.perceptionTarget.sprite = node.replacementSprite;
            }
        }
        else if (node.perceptionChangeType == PerceptionChangeType.Animation)
        {
            if (node.perceptionAnimator != null &&
                !string.IsNullOrEmpty(node.animationTriggerName))
            {
                node.perceptionAnimator.SetTrigger(
                    node.animationTriggerName
                );

                yield return new WaitForSeconds(
                    node.animationDuration
                );
            }
        }

        // Bring the dialogue back after the perception change.
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        perceptionEventRunning = false;

        // Continue automatically to the next dialogue node.
        if (nextNode == -1)
        {
            EndDialogue();
        }
        else
        {
            ShowNode(nextNode);
        }
    }

    private IEnumerator RunChoicePerceptionEvent(
    DialogueChoice choice)
    {
        perceptionEventRunning = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        yield return new WaitForSeconds(
            choice.perceptionDelay
        );

        if (choice.perceptionChangeType ==
            PerceptionChangeType.SpriteSwap)
        {
            if (choice.perceptionTarget != null &&
                choice.replacementSprite != null)
            {
                choice.perceptionTarget.sprite =
                    choice.replacementSprite;
            }
        }
        else if (choice.perceptionChangeType ==
                 PerceptionChangeType.Animation)
        {
            if (choice.perceptionAnimator != null &&
                !string.IsNullOrEmpty(
                    choice.animationTriggerName))
            {
                choice.perceptionAnimator.SetTrigger(
                    choice.animationTriggerName
                );

                yield return new WaitForSeconds(
                    choice.animationDuration
                );
            }
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        perceptionEventRunning = false;

        if (choice.nextNodeIndex == -1)
        {
            EndDialogue();
        }
        else
        {
            ShowNode(choice.nextNodeIndex);
        }
    }

    private IEnumerator RunNPCExitEvent(DialogueNode node)
{
    // Stop the dialogue immediately.
    waitingForChoice = false;

    if (dialoguePanel != null)
    {
        dialoguePanel.SetActive(false);
    }

    if (choicePanel != null)
    {
        choicePanel.SetActive(false);
    }

    // Make the NPC run to its exit point.
    if (node.npcExitMover != null)
    {
        yield return StartCoroutine(
            node.npcExitMover.RunToExit()
        );
    }

        EndDialogue();
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

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        perceptionEventRunning = false;

        completedPerceptionEvents.Clear();

        dialogueNodes = null;

        startingNodeIndex = 0;

        currentConversation = null;

        OnDialogueFinished?.Invoke();
OnDialogueFinished = null;
        
}
}