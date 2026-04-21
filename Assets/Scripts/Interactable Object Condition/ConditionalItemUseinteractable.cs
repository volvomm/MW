using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConditionalItemUseInteractable : MonoBehaviour, IInteractable, IIventoryItemRecivers
{
    [Header("Required Item")]
    [SerializeField] private InventoryItemData requiredItem;

    [Header("Door Setup")]
    [SerializeField] private GameObject holeBlockedSprite;
    [SerializeField] private SceneTransition sceneTransition;

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

    [Header("Flags")]
    [SerializeField] private bool setFirstDialogueFlag = true;
    [SerializeField] private string firstDialogueFlagName = "Door_Hint_Shown";

    [SerializeField] private bool setSuccessFlag = true;
    [SerializeField] private string successFlagName = "Door_Opened";

    [Header("Success Visual")]
    [SerializeField] private GameObject blockedVisual;

    [Header("Consume Item")]
    [SerializeField] private bool consumeItemOnSuccess = true;

    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private BoxDialogue currentDialogue;

    private bool openInventoryAfterDialogue = false;
    private bool setFlagAfterDialogue = false;
    private string pendingFlagName = "";

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (isDialogueActive)
        {
            NextLine();
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

    public void OnItemSelectedFromInventory(InventoryItemData selectedItem)
    {
        InventoryUIController inventoryUI = FindFirstObjectByType<InventoryUIController>();
        if (inventoryUI != null)
        {
            inventoryUI.CloseSelectionMode();
        }

        if (selectedItem == requiredItem)
        {
            HandleSuccess();
        }
        else
        {
            StartDialogue(failDialogue, false, false, "");
        }
    }

    private void HandleSuccess()
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

        if (sceneTransition != null)
        {
            sceneTransition.SetTransitionEnabled(true);
        }

        if (consumeItemOnSuccess && InventorySystem.Instance != null)
        {
            InventorySystem.Instance.RemoveItem(requiredItem);

            InventoryUIController inventoryUI = FindFirstObjectByType<InventoryUIController>();
            if (inventoryUI != null)
            {
                inventoryUI.RefreshUI();
            }
        }

        StartDialogue(successDialogue, false, false, "");
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

            if (shouldOpenInventory)
            {
                InventoryUIController inventoryUI = FindFirstObjectByType<InventoryUIController>();
                if (inventoryUI != null)
                {
                    inventoryUI.OpenForItemSelection(this);
                }
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