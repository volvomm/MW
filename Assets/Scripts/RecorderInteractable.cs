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
    [SerializeField] private TMP_Text recorderScreenText;

    [Header("Recording Dialogue")]
    [SerializeField] private BoxDialogue recordingDialogueData;
    [SerializeField] private BoxDialogue playbackDialogueData;
    [SerializeField] private BoxDialogue alreadyDoneDialogueData;

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

    private bool isRecording;
    private bool hasRecorded;
    private bool isPlayingAudio;

    private bool recorderSequenceComplete;
    private bool showingAlreadyDoneDialogue;

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

    private void SetRecorderScreenText(string newText)
    {
        if (recorderScreenText != null)
        {
            recorderScreenText.SetText(newText);
        }
    }

    public bool CanInteract()
    {
        return !inspectionOpen;
    }

    public void Interact()
    {
        // E continues any dialogue that is currently open.
        if (isDialogueActive)
        {
            NextLine();
            return;
        }

        // After the whole recorder puzzle has been completed,
        // interacting only shows Patch's reminder dialogue.
        if (recorderSequenceComplete)
        {
            StartAlreadyDoneDialogue();
            return;
        }

        // Do not reopen the interaction while the close-up is active.
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

    private void StartAlreadyDoneDialogue()
    {
        if (alreadyDoneDialogueData == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Already Done Dialogue Data is missing."
            );

            return;
        }

        showingAlreadyDoneDialogue = true;

        // Freeze Patch while this normal dialogue is visible.
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Do not show the recorder close-up.
        StartDialogue(alreadyDoneDialogueData);
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

        isRecording = false;
        hasRecorded = false;

        ShowPoweredScreen();
        SetRecorderScreenText("START");

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

        if (isPlayingAudio)
        {
            SetRecorderScreenText("PLAYING AUDIO");
        }
        else if (hasRecorded)
        {
            SetRecorderScreenText("RECORDED");
        }
        else if (isRecording)
        {
            SetRecorderScreenText("RECORDING");
        }
        else
        {
            SetRecorderScreenText("START");
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
        bool finishedPlaybackDialogue =
            currentDialogueData == playbackDialogueData;

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

        // This was the dialogue shown after returning to the
        // recorder once the puzzle had already been completed.
        if (showingAlreadyDoneDialogue)
        {
            showingAlreadyDoneDialogue = false;
            currentDialogueData = null;

            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }

            return;
        }

        // Patch has finished the dialogue after playing his recording.
        if (finishedPlaybackDialogue)
        {
            recorderSequenceComplete = true;
        }

        if (isRecording)
        {
            // Keep recording until the square Stop button is clicked.
            UpdateRecorderButtonStates();

            if (closeButton != null)
            {
                closeButton.SetActive(false);
            }

            return;
        }

        if (isPlayingAudio)
        {
            // The playback dialogue has finished.
            // Keep PLAYING AUDIO on the screen until X is clicked.
            UpdateRecorderButtonStates();

            if (closeButton != null)
            {
                closeButton.SetActive(true);
            }

            return;
        }

        UpdateRecorderButtonStates();

        if (closeButton != null)
        {
            closeButton.SetActive(true);
        }
    }

    public void PressRecordButton()
    {
        if (isDialogueActive)
        {
            return;
        }

        // Before the batteries are installed, clicking the button
        // still gives the old battery warning.
        if (!recorderPowered)
        {
            ShowBatteryDialogue();
            return;
        }

        // Do not begin another recording if one is already active
        // or a recording has already been completed.
        if (isRecording || hasRecorded)
        {
            return;
        }

        isRecording = true;

        SetRecorderScreenText("RECORDING");

        // Disable all controls while Patch is speaking.
        SetRecorderButtonsInteractable(false);

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        StartDialogue(recordingDialogueData);
    }

    public void PressStopButton()
    {
        if (isDialogueActive)
        {
            return;
        }

        if (!recorderPowered)
        {
            ShowBatteryDialogue();
            return;
        }

        // Stop only works after the red Record button has been used.
        if (!isRecording)
        {
            return;
        }

        isRecording = false;
        hasRecorded = true;

        SetRecorderScreenText("RECORDED");

        // There is no playback function yet, so disable the
        // recorder controls until we create the next step.
        UpdateRecorderButtonStates();

        if (closeButton != null)
        {
            closeButton.SetActive(true);
        }
    }

    public void PressPlayButton()
    {
        if (isDialogueActive)
        {
            return;
        }

        if (!recorderPowered)
        {
            ShowBatteryDialogue();
            return;
        }

        // The Play button only works once Patch has recorded audio.
        if (!hasRecorded || isPlayingAudio)
        {
            return;
        }

        isPlayingAudio = true;

        SetRecorderScreenText("PLAYING\nAUDIO");

        // Disable all controls while the playback dialogue is active.
        SetRecorderButtonsInteractable(false);

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        StartDialogue(playbackDialogueData);
    }

    private void UpdateRecorderButtonStates()
    {
        if (recordButton == null ||
            stopButton == null ||
            playButton == null)
        {
            return;
        }

        if (isDialogueActive)
        {
            recordButton.interactable = false;
            stopButton.interactable = false;
            playButton.interactable = false;
            return;
        }

        if (!recorderPowered)
        {
            recordButton.interactable = true;
            stopButton.interactable = true;
            playButton.interactable = true;
            return;
        }

        if (isRecording)
        {
            recordButton.interactable = false;
            stopButton.interactable = true;
            playButton.interactable = false;
            return;
        }

        if (isPlayingAudio)
        {
            recordButton.interactable = false;
            stopButton.interactable = false;
            playButton.interactable = false;
            return;
        }

        if (hasRecorded)
        {
            // Recording is complete, so only Play should work.
            recordButton.interactable = false;
            stopButton.interactable = false;
            playButton.interactable = true;
            return;
        }

        // Powered recorder before recording begins.
        recordButton.interactable = true;
        stopButton.interactable = false;
        playButton.interactable = false;
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