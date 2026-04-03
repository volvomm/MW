using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PuzzleInteractable : MonoBehaviour, IInteractable
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image portraitImage;

    [Header("Response (Top = Higher Priority)")]
    [SerializeField] private List<PuzzleInteractionResponse> response = new List<PuzzleInteractionResponse>();

    [Header("After Pickup")]
    [SerializeField] private bool disableOjectAfterPickup = true;
    [SerializeField] private bool destroyObjectAfterPickup = false;

    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;

    private PuzzleInteractionResponse currentResponse;

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (isDialogueActive)
        {
            NextLine();
            return;
        }

        currentResponse = GetVaildResponse();

        if (currentResponse == null)
        {
            Debug.LogWarning($"{gameObject.name}: theres no response that can start");
            return;
        }

        if (currentResponse.dialougeData == null)
        {
            CompleteInteraction();
            return;
        }

        StartDialogue(currentResponse.dialougeData);
    }

    private PuzzleInteractionResponse GetVaildResponse()
    {
        for (int i = 0; i < response.Count; i++)
        {
            if (response[i] != null && response[i].ConditionsMet())
                return response[i];
        }

        return null;
    }

    private void StartDialogue(BoxDialogue dialogueData)
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        if (nameText != null)
        {
            nameText.SetText(dialogueData.npcName);
        }
        if (portraitImage != null)
        {
            portraitImage.sprite = dialogueData.npcPortrait;
        }
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        StartCoroutine(TypeLine(dialogueData));
    }

    private void NextLine()
    {
        if (currentResponse == null || currentResponse.dialougeData == null)
            return;

        BoxDialogue dialogueData = currentResponse.dialougeData;

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine(dialogueData));
        }
        else
        {
            CompleteInteraction();
        }
    }

    private IEnumerator TypeLine(BoxDialogue dialogueData)
    {
        isTyping = true;

        if (dialogueData != null)
        {
            dialogueText.SetText("");
        }
        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            if (dialogueText != null)
            {
                dialogueText.text += letter;
            }
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

    private void CompleteInteraction()
    {
        EndDialogue();

        if (currentResponse == null)
            return;

        ExecuteActions(currentResponse.actionsAfterComplete);

        switch (currentResponse.resultType)
        {
            case InteractionResultType.DialougeOnly:
                break;

            case InteractionResultType.PickupItem:
                TryPickupItem(currentResponse.itemToPickup);
                break;
        }
    }

    private void ExecuteActions(List<FlagAction> actions)
    {
        if (actions == null) return;

        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i] != null)
                actions[i].Execute();
        }
    }

    private void TryPickupItem(InventoryItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning($"{gameObject.name}: pickup item is missing");
            return;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("There is no InventorySystem.Instance.");
            return;
        }

        bool added = InventorySystem.Instance.AddItem(itemData);

        if (!added)
            return;

        InventoryUIController ui = FindFirstObjectByType<InventoryUIController>();
        if (ui != null)
        {
            ui.RefreshUI();
        }

        if (destroyObjectAfterPickup)
        {
            Destroy(gameObject);
        }
        else if (disableOjectAfterPickup)
        {
            gameObject.SetActive(false);
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isTyping = false;
        isDialogueActive = false;

        if (dialogueText != null)
            dialogueText.SetText("");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

}
