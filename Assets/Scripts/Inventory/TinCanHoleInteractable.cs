using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TinCanHoleInteractable : MonoBehaviour, IInteractable, IIventoryItemRecivers
{
    [Header("Required Item")]
    [SerializeField] private InventoryItemData requiredItem;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image portraitImage;

    [Header("Dialogue Data")]
    [SerializeField] private BoxDialogue firstDialogue;
    [SerializeField] private BoxDialogue selectItemDialogue;
    [SerializeField] private BoxDialogue successDialogue;
    [SerializeField] private BoxDialogue failDialogue;

    [Header("Visual Change")]
    [SerializeField] private GameObject blockedVisual;

    [Header("Flags")]
    [SerializeField] private bool setSuccessFlag = true;
    [SerializeField] private string successFlagName = "Door_Opened";

    [Header("Consume Item")]
    [SerializeField] private bool consumeItemOnSuccess = true;

    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private bool openInventoryAfterDialogue;
    private BoxDialogue currentDialogue;

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
            StartDialogue(firstDialogue, false);
        }
        else
        {
            StartDialogue(selectItemDialogue, true);
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
            StartDialogue(failDialogue, false);
        }
    }

    private void HandleSuccess()
    {
        if (setSuccessFlag && !string.IsNullOrEmpty(successFlagName) && PuzzleFlagManager.Instance != null)
        {
            PuzzleFlagManager.Instance.SetFlag(successFlagName, true);
        }

        if (blockedVisual != null)
        {
            blockedVisual.SetActive(false);
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

        StartDialogue(successDialogue, false);
    }

    private void StartDialogue(BoxDialogue dialogueData, bool shouldOpenInventoryAfterDialogue)
    {
        if (dialogueData == null)
            return;

        currentDialogue = dialogueData;
        openInventoryAfterDialogue = shouldOpenInventoryAfterDialogue;

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
            bool shouldOpenInventory = openInventoryAfterDialogue;
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

        if (dialogueText != null)
            dialogueText.SetText("");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}