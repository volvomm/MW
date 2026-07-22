using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConditionalItemUseInteractable : MonoBehaviour, IInteractable, IIventoryItemRecivers
{
    [Header("Required Item")]
    [SerializeField] private InventoryItemData requiredItem;

    [Header("Door Setup")]
    [SerializeField] private GameObject blockedVisual;
    [SerializeField] private DoorChoiceTransition doorQuestionTransition;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image portraitImage;

    [Header("Dialogue Data")]
    [SerializeField] private BoxDialogue firstDialogue;
    [SerializeField] private BoxDialogue selectionDialogue;
    [SerializeField] private BoxDialogue successDialogue;
    [SerializeField] private BoxDialogue failDialogue;
    [SerializeField] private BoxDialogue alreadyOpenedDialogue;

    [Header("Flags")]
    [SerializeField] private bool setFirstDialogueFlag = true;
    [SerializeField] private string firstDialogueFlagName = "Door_Hint_Shown";

    [SerializeField] private bool setSuccessFlag = true;
    [SerializeField] private string successFlagName = "Door_Opened";

    [Header("Consume Item")]
    [SerializeField] private bool consumeItemOnSuccess = true;

    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private BoxDialogue currentDialogue;

    private bool openInventoryAfterDialogue = false;
    private bool setFlagAfterDialogue = false;
    private string pendingFlagName = "";

    private InventoryUIController inventoryUI;

    public SceneTransition sceneTransition;

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    private void Awake()
    {
        inventoryUI = FindFirstObjectByType<InventoryUIController>();
    }

    public void Interact()
    {
        if (isDialogueActive)
        {
            NextLine();
            return;
        }

        if (IsAlreadyOpened())
        {
            if (alreadyOpenedDialogue != null)
                StartDialogue(alreadyOpenedDialogue, false, false, "");

            return;
        }

        if (InventorySystem.Instance == null)
            return;

        bool hasRequiredItem = InventorySystem.Instance.HasItem(requiredItem);

        if (!hasRequiredItem)
        {
            StartDialogue(
                firstDialogue,
                shouldOpenInventoryAfterDialogue: false,
                shouldSetFlagAfterDialogue: setFirstDialogueFlag,
                flagNameToSet: firstDialogueFlagName
            );
        }
        else
        {
            StartDialogue(
                selectionDialogue,
                shouldOpenInventoryAfterDialogue: true,
                shouldSetFlagAfterDialogue: false,
                flagNameToSet: ""
            );
        }
    }

    public void DialogueStart()
    {
        StartDialogue(successDialogue, false, false, "");
        return;
    }

    public void OnItemSelectedFromInventory(InventoryItemData selectedItem)
    {
        if (inventoryUI != null)
        {
            inventoryUI.CloseSelectionMode();
        }

        if (selectedItem != requiredItem)
        {
            StartDialogue(failDialogue, false, false, "");
        }
    }

    public void HandleSuccess()
    {
        if (setSuccessFlag &&
            !string.IsNullOrEmpty(successFlagName) &&
            PuzzleFlagManager.Instance != null)
        {
            PuzzleFlagManager.Instance.SetFlag(successFlagName, true);
        }

        if (blockedVisual != null)
        {
            blockedVisual.SetActive(false);
        }

        if (doorQuestionTransition != null)
        {
            doorQuestionTransition.enabled = true;
        }

        if (consumeItemOnSuccess && InventorySystem.Instance != null)
        {
            InventorySystem.Instance.RemoveItem(requiredItem);

            if (inventoryUI != null)
            {
                inventoryUI.RefreshUI();
            }
        }
        return;
    }

    private bool IsAlreadyOpened()
    {
        if (!setSuccessFlag || string.IsNullOrEmpty(successFlagName))
            return false;

        if (PuzzleFlagManager.Instance == null)
            return false;

        return PuzzleFlagManager.Instance.GetFlag(successFlagName);
    }

    private void StartDialogue(
        BoxDialogue dialogueData,
        bool shouldOpenInventoryAfterDialogue,
        bool shouldSetFlagAfterDialogue,
        string flagNameToSet)
    {
        if (dialogueData == null)
            return;

        currentDialogue = dialogueData;
        openInventoryAfterDialogue = shouldOpenInventoryAfterDialogue;
        setFlagAfterDialogue = shouldSetFlagAfterDialogue;
        pendingFlagName = flagNameToSet;

        isDialogueActive = true;
        dialogueIndex = 0;

        if (nameText != null)
            nameText.SetText(dialogueData.npcName);

        if (portraitImage != null)
            portraitImage.sprite = dialogueData.npcPortrait;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        StartCoroutine(TypeLine(dialogueData));
    }

    private void NextLine()
    {
        if (currentDialogue == null)
            return;

        if (isTyping)
        {
            StopAllCoroutines();

            if (dialogueText != null)
                dialogueText.SetText(currentDialogue.dialogueLines[dialogueIndex]);

            isTyping = false;
        }
        else if (++dialogueIndex < currentDialogue.dialogueLines.Length)
        {
            StartCoroutine(TypeLine(currentDialogue));
        }
        else
        {
            bool shouldSetFlag = setFlagAfterDialogue;
            string flagToSet = pendingFlagName;
            bool shouldOpenInventory = openInventoryAfterDialogue;

            if (shouldSetFlag &&
                !string.IsNullOrEmpty(flagToSet) &&
                PuzzleFlagManager.Instance != null)
            {
                PuzzleFlagManager.Instance.SetFlag(flagToSet, true);
            }

            EndDialogue();

            if (shouldOpenInventory && inventoryUI != null)
            {
                inventoryUI.OpenForItemSelection(this);
            }
        }
    }

    private IEnumerator TypeLine(BoxDialogue dialogueData)
    {
        isTyping = true;

        if (dialogueText != null)
            dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            if (dialogueText != null)
                dialogueText.text += letter;

            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (dialogueData.autoProgressLines != null &&
            dialogueData.autoProgressLines.Length > dialogueIndex &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    private void EndDialogue()
    {
        StopAllCoroutines();
        isTyping = false;
        isDialogueActive = false;
        openInventoryAfterDialogue = false;
        setFlagAfterDialogue = false;
        pendingFlagName = "";

        if (dialogueText != null)
            dialogueText.SetText("");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}