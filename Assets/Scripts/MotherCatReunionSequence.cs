using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MotherCatReunionSequence : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class DialogueLine
    {
        [Header("Speaker")]
        public string speakerName;

        [TextArea(2, 5)]
        public string dialogueText;

        public Sprite speakerPortrait;
    }

    [Header("Basement Dialogue")]
    [SerializeField]
    private DialogueLine[] basementDialogueLines;

    [Header("Reunion Dialogue")]
    [SerializeField]
    private DialogueLine[] reunionDialogueLines;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject closeButton;

    [Header("Player")]
    [SerializeField] private GameObject patch;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Animator playerAnimator;

    [Header("Mother Cat")]
    [SerializeField] private GameObject motherCat;

    [Header("Reunion Positions")]
    [SerializeField] private Transform patchReunionPosition;
    [SerializeField] private Transform motherReunionPosition;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform kittenRoomCameraPoint;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeSpeed = 1f;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.04f;

    private enum DialogueStage
    {
        None,
        Basement,
        Reunion
    }

    private DialogueStage currentStage = DialogueStage.None;

    private int currentLineIndex;

    private bool sequenceActive;
    private bool isTyping;
    private bool waitingForInput;

    private Coroutine typingCoroutine;
    private Coroutine transitionCoroutine;

    private string currentCompleteText;

    public bool CanInteract()
    {
        // Mother Cat can only start this new conversation:
        // 1. after she has already been rescued,
        // 2. after the closet has been barricaded,
        // 3. before the reunion has already happened,
        // 4. while this sequence is not already active.

        return StoryProgress.MotherCatRescueDialogueFinished
               && StoryProgress.ClosetBarricaded
               && !StoryProgress.MotherCatReunited
               && !sequenceActive;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        BeginBasementDialogue();
    }

    private void Update()
    {
        if (!sequenceActive)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        // Do not allow E to interfere while the screen
        // is currently transitioning between rooms.
        if (currentStage == DialogueStage.None)
        {
            return;
        }

        if (isTyping)
        {
            CompleteCurrentLineImmediately();
            return;
        }

        if (!waitingForInput)
        {
            return;
        }

        waitingForInput = false;

        AdvanceDialogue();
    }

    private void BeginBasementDialogue()
    {
        if (basementDialogueLines == null ||
            basementDialogueLines.Length == 0)
        {
            Debug.LogWarning(
                "MotherCatReunionSequence: No basement dialogue has been assigned.",
                this
            );

            return;
        }

        sequenceActive = true;
        currentStage = DialogueStage.Basement;
        currentLineIndex = 0;

        FreezePlayer();

        OpenDialoguePanel();

        ShowCurrentLine();
    }

    private void BeginReunionDialogue()
    {
        if (reunionDialogueLines == null ||
            reunionDialogueLines.Length == 0)
        {
            Debug.LogWarning(
                "MotherCatReunionSequence: No reunion dialogue has been assigned.",
                this
            );

            FinishEntireSequence();
            return;
        }

        currentStage = DialogueStage.Reunion;
        currentLineIndex = 0;

        OpenDialoguePanel();

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        DialogueLine[] activeLines = GetCurrentDialogueLines();

        if (activeLines == null ||
            currentLineIndex < 0 ||
            currentLineIndex >= activeLines.Length)
        {
            return;
        }

        DialogueLine currentLine =
            activeLines[currentLineIndex];

        if (speakerNameText != null)
        {
            speakerNameText.text =
                currentLine.speakerName;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite =
                currentLine.speakerPortrait;

            portraitImage.enabled =
                currentLine.speakerPortrait != null;
        }

        currentCompleteText =
    currentLine.dialogueText;

        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        StartTypingCurrentLine();
    }

    private DialogueLine[] GetCurrentDialogueLines()
    {
        if (currentStage == DialogueStage.Basement)
        {
            return basementDialogueLines;
        }

        if (currentStage == DialogueStage.Reunion)
        {
            return reunionDialogueLines;
        }

        return null;
    }

    private void StartTypingCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        typingCoroutine =
            StartCoroutine(TypeCurrentLine());
    }

    private IEnumerator TypeCurrentLine()
    {
        isTyping = true;
        waitingForInput = false;

        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = int.MaxValue;
            dialogueText.text = "";
        }

        foreach (char letter in currentCompleteText)
        {
            if (dialogueText != null)
            {
                dialogueText.text += letter;
            }

            yield return new WaitForSeconds(
                typingSpeed
            );
        }

        if (dialogueText != null)
        {
            dialogueText.text =
                currentCompleteText;
        }

        isTyping = false;
        waitingForInput = true;
        typingCoroutine = null;
    }

    private void CompleteCurrentLineImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = int.MaxValue;
            dialogueText.text = currentCompleteText;
        }

        isTyping = false;
        waitingForInput = true;
    }

    private void AdvanceDialogue()
    {
        DialogueLine[] activeLines =
            GetCurrentDialogueLines();

        if (activeLines == null)
        {
            return;
        }

        currentLineIndex++;

        if (currentLineIndex <
            activeLines.Length)
        {
            ShowCurrentLine();
            return;
        }

        // The basement conversation has finished.
        if (currentStage ==
            DialogueStage.Basement)
        {
            EndBasementDialogue();
            return;
        }

        // The kitten reunion conversation has finished.
        if (currentStage ==
            DialogueStage.Reunion)
        {
            FinishEntireSequence();
        }
    }

    private void EndBasementDialogue()
    {
        CloseDialoguePanel();

        currentStage = DialogueStage.None;

        if (transitionCoroutine != null)
        {
            StopCoroutine(
                transitionCoroutine
            );
        }

        transitionCoroutine =
            StartCoroutine(
                TransitionToKittenRoom()
            );
    }

    private IEnumerator TransitionToKittenRoom()
    {
        // Patch stays frozen during this entire transition.

        yield return StartCoroutine(
            Fade(1f)
        );

        // -------------------------------------------------
        // SCREEN IS NOW COMPLETELY BLACK.
        // Move everything while the player cannot see it.
        // -------------------------------------------------

        if (patch != null &&
            patchReunionPosition != null)
        {
            patch.transform.position =
                patchReunionPosition.position;
        }

        if (motherCat != null &&
            motherReunionPosition != null)
        {
            motherCat.transform.position =
                motherReunionPosition.position;
        }

        if (mainCamera != null &&
            kittenRoomCameraPoint != null)
        {
            mainCamera.transform.position =
                new Vector3(
                    kittenRoomCameraPoint.position.x,
                    kittenRoomCameraPoint.position.y,
                    mainCamera.transform.position.z
                );
        }

        SetPatchFacingLeft();
        SetMotherFacingRight();

        // Make sure neither character has movement velocity.
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;
        }

        yield return StartCoroutine(
            Fade(0f)
        );

        transitionCoroutine = null;

        // Immediately begin the family reunion dialogue.
        BeginReunionDialogue();
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadePanel == null)
        {
            yield break;
        }

        fadePanel.gameObject.SetActive(true);

        while (!Mathf.Approximately(
                   fadePanel.alpha,
                   targetAlpha))
        {
            fadePanel.alpha =
                Mathf.MoveTowards(
                    fadePanel.alpha,
                    targetAlpha,
                    fadeSpeed *
                    Time.deltaTime
                );

            yield return null;
        }

        fadePanel.alpha = targetAlpha;

        if (targetAlpha <= 0f)
        {
            fadePanel.gameObject.SetActive(false);
        }
    }

    private void OpenDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }
    }

    private void CloseDialoguePanel()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(
                typingCoroutine
            );

            typingCoroutine = null;
        }

        isTyping = false;
        waitingForInput = false;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

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
            playerMovement
                .StopMovementImmediately();

            playerMovement.enabled =
                false;
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
            playerMovement
                .StopMovementImmediately();

            playerMovement.enabled =
                true;
        }
    }

    private void SetPatchFacingLeft()
    {
        if (patch == null)
        {
            return;
        }

        SpriteRenderer patchRenderer =
            patch.GetComponent<SpriteRenderer>();

        if (patchRenderer != null)
        {
            patchRenderer.flipX = true;
        }
    }

    private void SetMotherFacingRight()
    {
        if (motherCat == null)
        {
            return;
        }

        SpriteRenderer motherRenderer =
            motherCat.GetComponent<SpriteRenderer>();

        if (motherRenderer != null)
        {
            motherRenderer.flipX = false;
        }
    }

    private void FinishEntireSequence()
    {
        CloseDialoguePanel();

        currentStage =
            DialogueStage.None;

        sequenceActive = false;

        StoryProgress.MotherCatReunited =
            true;

        if (closeButton != null)
        {
            closeButton.SetActive(true);
        }

        UnfreezePlayer();
    }
}