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

    [Header("Player Movement Lock")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Animator playerAnimator;

    [Header("Response (Top = Higher Priority)")]
    [SerializeField]
    private List<PuzzleInteractionResponse> response =
        new List<PuzzleInteractionResponse>();

    [Header("After Pickup")]
    [SerializeField] private bool disableOjectAfterPickup = true;
    [SerializeField] private bool destroyObjectAfterPickup = false;

    private int dialogueIndex = 0;

    private bool isTyping = false;
    private bool isDialogueActive = false;
    private bool interactionCompleting = false;

    private PuzzleInteractionResponse currentResponse;

    private Coroutine typingCoroutine;

    // =========================================================
    // INTERACTION
    // =========================================================

    public bool CanInteract()
    {
        // E must still be allowed while dialogue is active
        // so Patch can reveal/advance the dialogue.
        if (isDialogueActive)
        {
            return true;
        }

        return !interactionCompleting;
    }

    public void Interact()
    {
        // If dialogue is already active,
        // E advances the current dialogue.
        if (isDialogueActive)
        {
            NextLine();
            return;
        }

        if (interactionCompleting)
        {
            return;
        }

        currentResponse = GetValidResponse();

        if (currentResponse == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: there is no response that can start."
            );

            return;
        }

        // If this response has no dialogue,
        // immediately complete its action.
        if (currentResponse.dialougeData == null)
        {
            CompleteInteraction();
            return;
        }

        StartDialogue(currentResponse.dialougeData);
    }

    // =========================================================
    // RESPONSE
    // =========================================================

    private PuzzleInteractionResponse GetValidResponse()
    {
        for (int i = 0; i < response.Count; i++)
        {
            if (response[i] != null &&
                response[i].ConditionsMet())
            {
                return response[i];
            }
        }

        return null;
    }

    // =========================================================
    // START DIALOGUE
    // =========================================================

    private void StartDialogue(BoxDialogue dialogueData)
    {
        if (dialogueData == null)
        {
            return;
        }

        // Make sure there is actually dialogue to display.
        if (dialogueData.dialogueLines == null ||
            dialogueData.dialogueLines.Length == 0)
        {
            CompleteInteraction();
            return;
        }

        isDialogueActive = true;
        interactionCompleting = false;
        isTyping = false;

        dialogueIndex = 0;

        // Stop Patch from moving during dialogue.
        FreezePlayer();

        if (nameText != null)
        {
            nameText.SetText(dialogueData.npcName);
        }

        if (portraitImage != null)
        {
            portraitImage.sprite =
                dialogueData.npcPortrait;

            portraitImage.enabled =
                dialogueData.npcPortrait != null;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // IMPORTANT FIX:
        //
        // Another dialogue system may have left
        // maxVisibleCharacters at a low number.
        //
        // Reset it so the shoebox dialogue cannot
        // be visually cut off.
        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = 9999;
        }

        StartTypingCurrentLine(dialogueData);
    }

    // =========================================================
    // DIALOGUE INPUT
    // =========================================================

    private void NextLine()
    {
        if (!isDialogueActive)
        {
            return;
        }

        if (currentResponse == null ||
            currentResponse.dialougeData == null)
        {
            return;
        }

        BoxDialogue dialogueData =
            currentResponse.dialougeData;

        if (dialogueData.dialogueLines == null ||
            dialogueData.dialogueLines.Length == 0)
        {
            CompleteInteraction();
            return;
        }

        // =====================================================
        // CURRENT LINE IS STILL TYPING
        // =====================================================

        // Pressing E while typing reveals the ENTIRE
        // current sentence.
        //
        // It does NOT move to the next sentence yet.
        if (isTyping)
        {
            CompleteCurrentLineImmediately(
                dialogueData
            );

            return;
        }

        // =====================================================
        // CURRENT LINE IS ALREADY COMPLETELY VISIBLE
        // =====================================================

        dialogueIndex++;

        // If another line exists, start typing it.
        if (dialogueIndex <
            dialogueData.dialogueLines.Length)
        {
            StartTypingCurrentLine(
                dialogueData
            );

            return;
        }

        // There are no more dialogue lines.
        // The interaction can now finish.
        CompleteInteraction();
    }

    // =========================================================
    // TYPEWRITER
    // =========================================================

    private void StartTypingCurrentLine(
        BoxDialogue dialogueData)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        typingCoroutine =
            StartCoroutine(
                TypeLine(dialogueData)
            );
    }

    private IEnumerator TypeLine(
        BoxDialogue dialogueData)
    {
        isTyping = true;

        if (dialogueText != null)
        {
            dialogueText.SetText("");

            // IMPORTANT FIX:
            // Make sure the TMP character visibility limit
            // has not been left at a smaller value.
            dialogueText.maxVisibleCharacters = 9999;
        }

        string line =
            dialogueData.dialogueLines[
                dialogueIndex
            ];

        if (line == null)
        {
            line = "";
        }

        // Type the sentence one character at a time.
        foreach (char letter in line)
        {
            if (dialogueText != null)
            {
                dialogueText.text += letter;

                // Keep all typed characters visible.
                dialogueText.maxVisibleCharacters = 9999;
            }

            yield return new WaitForSeconds(
                dialogueData.typingSpeed
            );
        }

        // Make absolutely sure the FULL sentence
        // is visible after typing finishes.
        if (dialogueText != null)
        {
            dialogueText.SetText(line);

            // Same type of fix your teacher used.
            dialogueText.maxVisibleCharacters = 9999;
        }

        isTyping = false;
        typingCoroutine = null;

        // =====================================================
        // OPTIONAL AUTO PROGRESS
        // =====================================================

        if (dialogueData.autoProgressLines != null &&
            dialogueData.autoProgressLines.Length >
            dialogueIndex &&
            dialogueData.autoProgressLines[
                dialogueIndex
            ])
        {
            yield return new WaitForSeconds(
                dialogueData.autoProgressDelay
            );

            if (isDialogueActive)
            {
                NextLine();
            }
        }
    }

    // =========================================================
    // COMPLETE CURRENT SENTENCE IMMEDIATELY
    // =========================================================

    private void CompleteCurrentLineImmediately(
        BoxDialogue dialogueData)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            // Display the ENTIRE current sentence.
            dialogueText.SetText(
                dialogueData.dialogueLines[
                    dialogueIndex
                ]
            );

            // IMPORTANT FIX:
            //
            // This is equivalent to the fix your teacher
            // used, except 9999 is used instead of 100
            // so even long dialogue lines are safe.
            dialogueText.maxVisibleCharacters = 9999;
        }

        isTyping = false;
    }

    // =========================================================
    // COMPLETE INTERACTION
    // =========================================================

    private void CompleteInteraction()
    {
        // Prevent this interaction from completing twice.
        if (interactionCompleting)
        {
            return;
        }

        interactionCompleting = true;

        // Save the response before doing anything
        // that could disable this GameObject.
        PuzzleInteractionResponse completedResponse =
            currentResponse;

        // Finish the dialogue FIRST.
        EndDialogue();

        if (completedResponse == null)
        {
            interactionCompleting = false;
            return;
        }

        // Execute story/flag actions.
        ExecuteActions(
            completedResponse.actionsAfterComplete
        );

        switch (completedResponse.resultType)
        {
            case InteractionResultType.DialougeOnly:

                interactionCompleting = false;
                break;

            case InteractionResultType.PickupItem:

                TryPickupItem(
                    completedResponse.itemToPickup
                );

                break;
        }
    }

    // =========================================================
    // ACTIONS
    // =========================================================

    private void ExecuteActions(
        List<FlagAction> actions)
    {
        if (actions == null)
        {
            return;
        }

        for (int i = 0;
             i < actions.Count;
             i++)
        {
            if (actions[i] != null)
            {
                actions[i].Execute();
            }
        }
    }

    // =========================================================
    // ITEM PICKUP
    // =========================================================

    private void TryPickupItem(
        InventoryItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: pickup item is missing."
            );

            interactionCompleting = false;
            return;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning(
                "There is no InventorySystem.Instance."
            );

            interactionCompleting = false;
            return;
        }

        bool added =
            InventorySystem.Instance.AddItem(
                itemData
            );

        if (!added)
        {
            interactionCompleting = false;
            return;
        }

        InventoryUIController ui =
            FindFirstObjectByType<
                InventoryUIController
            >();

        if (ui != null)
        {
            ui.RefreshUI();
        }

        // Everything involving dialogue has finished
        // BEFORE we disable/destroy this GameObject.
        if (destroyObjectAfterPickup)
        {
            Destroy(gameObject);
            return;
        }

        if (disableOjectAfterPickup)
        {
            gameObject.SetActive(false);
            return;
        }

        interactionCompleting = false;
    }

    // =========================================================
    // END DIALOGUE
    // =========================================================

    public void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        isDialogueActive = false;

        dialogueIndex = 0;

        if (dialogueText != null)
        {
            dialogueText.SetText("");

            // Leave TMP ready for the next dialogue.
            dialogueText.maxVisibleCharacters = 9999;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Give control back to Patch.
        UnfreezePlayer();
    }

    // =========================================================
    // PLAYER MOVEMENT
    // =========================================================

    private void FreezePlayer()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat(
                "Speed",
                0f
            );
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
            playerAnimator.SetFloat(
                "Speed",
                0f
            );
        }

        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
            playerMovement.enabled = true;
        }
    }
}