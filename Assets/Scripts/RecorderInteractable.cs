using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecorderInteractable : MonoBehaviour, IInteractable
{
    [Header("Recorder Inspection")]
    [SerializeField] private RecorderInspection recorderInspection;
    [SerializeField] private GameObject closeButton;

    [Header("Powered Screen UI")]
    [SerializeField] private GameObject recorderScreenGlow;
    [SerializeField] private GameObject recorderStartText;

    [Header("Recorder Clickable Buttons")]
    [SerializeField] private Button recordButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button playButton;

    [Header("Dialogue")]
    [SerializeField] private BoxDialogue initialDialogueData;
    [SerializeField] private BoxDialogue batteryDialogueData;
    [SerializeField] private BoxDialogue poweredDialogueData;

    [Header("Battery Item")]
    [SerializeField] private InventoryItemData batteriesItem;

    [Header("Dialogue UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image portraitImage;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    private BoxDialogue currentDialogueData;

    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private bool inspectionOpen;
    private bool recorderPowered;

    private void Start()
    {
        SetRecorderButtonsInteractable(false);

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        if (recorderScreenGlow != null)
        {
            recorderScreenGlow.SetActive(false);
        }

        if (recorderStartText != null)
        {
            recorderStartText.SetActive(false);
        }

    }

    public bool CanInteract()
    {
        return !inspectionOpen;
    }

    public void Interact()
    {
        if (isDialogueActive)
        {
            NextLine();
            return;
        }

        if (inspectionOpen)
        {
            return;
        }

        if (initialDialogueData == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Initial recorder dialogue is missing."
            );

            return;
        }

        OpenRecorderInspection();
    }

    private void OpenRecorderInspection()
    {
        inspectionOpen = true;

        if (!recorderPowered)
        {
            if (recorderScreenGlow != null)
            {
                recorderScreenGlow.SetActive(false);
            }

            if (recorderStartText != null)
            {
                recorderStartText.SetActive(false);
            }
        }

        SetRecorderButtonsInteractable(false);

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        if (recorderInspection != null)
        {
            recorderInspection.ShowRecorder();
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // If the recorder was already powered earlier,
        // use the powered dialogue again.
        if (recorderPowered)
        {
            ShowPoweredScreen();
            StartDialogue(poweredDialogueData);
            return;
        }

        // Check whether Patch currently owns the batteries.
        bool hasBatteries =
            InventorySystem.Instance != null &&
            InventorySystem.Instance.HasItem(batteriesItem);

        if (hasBatteries)
        {
            PowerRecorder();
            return;
        }

        // Patch does not have batteries yet.
        StartDialogue(initialDialogueData);
    }

    private void PowerRecorder()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("InventorySystem.Instance was not found.");
            StartDialogue(initialDialogueData);
            return;
        }

        if (batteriesItem == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Batteries Item has not been assigned."
            );

            StartDialogue(initialDialogueData);
            return;
        }

        bool removed =
            InventorySystem.Instance.RemoveItem(batteriesItem);

        if (!removed)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Batteries could not be removed from inventory."
            );

            StartDialogue(initialDialogueData);
            return;
        }

        recorderPowered = true;
        ShowPoweredScreen();

        // Refresh the inventory so the battery icon disappears.
        InventoryUIController inventoryUI =
            FindFirstObjectByType<InventoryUIController>();

        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        StartDialogue(poweredDialogueData);
    }

    private void StartDialogue(BoxDialogue dialogueData)
    {
        if (dialogueData == null)
        {
            return;
        }

        currentDialogueData = dialogueData;
        isDialogueActive = true;
        dialogueIndex = 0;

        SetRecorderButtonsInteractable(false);

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        if (nameText != null)
        {
            nameText.SetText(currentDialogueData.npcName);
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = currentDialogueData.npcPortrait;
            portraitImage.enabled =
                currentDialogueData.npcPortrait != null;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        StartCoroutine(TypeLine());
    }

    private void ShowPoweredScreen()
    {
        if (recorderScreenGlow != null)
        {
            recorderScreenGlow.SetActive(true);
        }

        if (recorderStartText != null)
        {
            recorderStartText.SetActive(true);
        }
    }

    private void NextLine()
    {
        if (currentDialogueData == null ||
            currentDialogueData.dialogueLines == null ||
            currentDialogueData.dialogueLines.Length == 0)
        {
            FinishCurrentDialogue();
            return;
        }

        if (isTyping)
        {
            StopAllCoroutines();

            if (dialogueText != null)
            {
                dialogueText.SetText(
                    currentDialogueData.dialogueLines[dialogueIndex]
                );
            }

            isTyping = false;
            return;
        }

        dialogueIndex++;

        if (dialogueIndex <
            currentDialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            FinishCurrentDialogue();
        }
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;

        if (dialogueText != null)
        {
            dialogueText.SetText("");
        }

        string currentLine =
            currentDialogueData.dialogueLines[dialogueIndex];

        foreach (char letter in currentLine)
        {
            if (dialogueText != null)
            {
                dialogueText.text += letter;
            }

            yield return new WaitForSeconds(
                currentDialogueData.typingSpeed
            );
        }

        isTyping = false;

        if (currentDialogueData.autoProgressLines != null &&
            currentDialogueData.autoProgressLines.Length >
            dialogueIndex &&
            currentDialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(
                currentDialogueData.autoProgressDelay
            );

            NextLine();
        }
    }

    private void FinishCurrentDialogue()
    {
        StopAllCoroutines();

        isTyping = false;
        isDialogueActive = false;
        dialogueIndex = 0;

        if (dialogueText != null)
        {
            dialogueText.SetText("");
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Always keep the close-up recorder visible.
        // The X returns after every dialogue.
        if (closeButton != null)
        {
            closeButton.SetActive(true);
        }

        if (recorderPowered)
        {
            // Leave the controls disabled for now.
            // We will replace them with the functional-recorder UI next.
            SetRecorderButtonsInteractable(false);
        }
        else
        {
            // Without batteries, buttons can still be tested.
            SetRecorderButtonsInteractable(true);
        }
    }

    public void PressRecordButton()
    {
        ShowBatteryDialogue();
    }

    public void PressStopButton()
    {
        ShowBatteryDialogue();
    }

    public void PressPlayButton()
    {
        ShowBatteryDialogue();
    }

    private void ShowBatteryDialogue()
    {
        if (isDialogueActive || recorderPowered)
        {
            return;
        }

        SetRecorderButtonsInteractable(false);

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        StartDialogue(batteryDialogueData);
    }
    private void SetRecorderButtonsInteractable(bool interactable)
    {
        if (recordButton != null)
        {
            recordButton.interactable = interactable;
        }

        if (stopButton != null)
        {
            stopButton.interactable = interactable;
        }

        if (playButton != null)
        {
            playButton.interactable = interactable;
        }
    }

    public void CloseInspectionFromButton()
    {
        if (isDialogueActive)
        {
            return;
        }

        StopAllCoroutines();

        SetRecorderButtonsInteractable(false);

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (recorderInspection != null)
        {
            recorderInspection.HideRecorder();
        }

        inspectionOpen = false;
        isTyping = false;
        isDialogueActive = false;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        inspectionOpen = false;
        isTyping = false;
        isDialogueActive = false;

        SetRecorderButtonsInteractable(false);

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        if (recorderInspection != null)
        {
            recorderInspection.HideRecorder();
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }
}