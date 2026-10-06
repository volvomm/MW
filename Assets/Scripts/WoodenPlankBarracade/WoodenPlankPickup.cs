using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WoodenPlankPickup : MonoBehaviour, IInteractable
{
    [Header("Plank Order")]
    [Tooltip("WoodFloor1 = 1, WoodFloor2 = 2, WoodFloor3 = 3")]
    [SerializeField] private int plankNumber = 1;

    [Header("Inventory")]
    [SerializeField] private InventoryItemData woodenPlankItem;

    [Header("Plank QTE")]
    [SerializeField] private PlankQTEManager plankQTEManager;

    [Header("WoodFloor1 Dialogue")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private Sprite patchPortrait;
    [SerializeField] private GameObject closeButton;

    [TextArea(3, 6)]
    [SerializeField]
    private string firstPlankDialogue =
        "I need to put this against the closet door to keep it shut, just need to press the correct buttons.";

    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Player Movement")]
    [SerializeField] private PlayerMovement playerMovement;

    private bool plankTaken;
    private bool qteInProgress;

    private bool dialogueActive;
    private bool isTyping;
    private bool firstDialogueFinished;

    private Coroutine typingCoroutine;

    public bool CanInteract()
    {
        if (!StoryProgress.DevilDogTrapdoorSequenceFinished)
        {
            return false;
        }

        if (plankTaken || qteInProgress || dialogueActive)
        {
            return false;
        }

        // Controls which floor plank is currently allowed.
        //
        // 0 planks placed = WoodFloor1
        // 1 plank placed  = WoodFloor2
        // 2 planks placed = WoodFloor3
        int requiredPlankNumber =
            StoryProgress.ClosetPlanksPlaced + 1;

        return plankNumber == requiredPlankNumber;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        if (plankQTEManager == null)
        {
            Debug.LogWarning(
                "WoodenPlankPickup: Plank QTE Manager has not been assigned."
            );
            return;
        }

        // Only WoodFloor1 gets the introductory dialogue.
        if (plankNumber == 1 && !firstDialogueFinished)
        {
            StartFirstPlankDialogue();
            return;
        }

        // WoodFloor2 and WoodFloor3 go directly to the QTE.
        StartPlankQTE();
    }

    private void StartFirstPlankDialogue()
    {
        dialogueActive = true;
        isTyping = true;

        // Freeze Patch.
        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
            playerMovement.enabled = false;
        }

        // Show dialogue panel.
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // IMPORTANT:
        // Hide the X button for this dialogue.
        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        if (speakerNameText != null)
        {
            speakerNameText.text = "Patch";
        }

        if (speakerPortraitImage != null)
        {
            speakerPortraitImage.sprite = patchPortrait;
            speakerPortraitImage.enabled =
                patchPortrait != null;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypeFirstPlankDialogue());
    }

    private IEnumerator TypeFirstPlankDialogue()
    {
        if (dialogueText == null)
        {
            isTyping = false;
            typingCoroutine = null;
            yield break;
        }

        // Start completely empty.
        dialogueText.text = "";

        // Add one character at a time.
        foreach (char letter in firstPlankDialogue)
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        // Make sure the complete sentence is displayed.
        dialogueText.text = firstPlankDialogue;

        isTyping = false;
        typingCoroutine = null;
    }

    private void Update()
    {
        if (!dialogueActive)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        // FIRST E:
        // If text is still typing, reveal everything.
        if (isTyping)
        {
            CompleteDialogueImmediately();
            return;
        }

        // SECOND E:
        // If the full sentence is already visible,
        // close dialogue and begin QTE.
        FinishFirstPlankDialogue();
    }

    private void CompleteDialogueImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = firstPlankDialogue;
        }

        isTyping = false;
    }

    private void FinishFirstPlankDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueActive = false;
        isTyping = false;
        firstDialogueFinished = true;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Restore the X button so other dialogue systems
        // can still use it later if they need it.
        if (closeButton != null)
        {
            closeButton.SetActive(true);
        }

        // Do NOT restore movement here.
        // The QTE is starting immediately and its manager
        // handles freezing/unfreezing Patch.
        StartPlankQTE();
    }

    private void StartPlankQTE()
    {
        qteInProgress = true;

        plankQTEManager.StartQTE(this);
    }

    public void QTESucceeded()
    {
        qteInProgress = false;

        if (plankTaken)
        {
            return;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning(
                "WoodenPlankPickup: InventorySystem.Instance was not found."
            );
            return;
        }

        if (woodenPlankItem == null)
        {
            Debug.LogWarning(
                "WoodenPlankPickup: Wooden Plank Item has not been assigned."
            );
            return;
        }

        bool added =
            InventorySystem.Instance.AddItem(woodenPlankItem);

        if (!added)
        {
            return;
        }

        plankTaken = true;

        InventoryUIController inventoryUI =
            FindFirstObjectByType<InventoryUIController>();

        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        Debug.Log(
            gameObject.name +
            " successfully collected! Plank number: " +
            plankNumber
        );

        gameObject.SetActive(false);
    }

    public void QTEFailed()
    {
        qteInProgress = false;

        Debug.Log(
            gameObject.name +
            " QTE failed. Patch can retry."
        );
    }
}