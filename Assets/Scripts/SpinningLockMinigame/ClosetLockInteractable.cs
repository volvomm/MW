using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClosetLockInteractable : MonoBehaviour, IInteractable
{
    [Header("Minigame")]
    [SerializeField] private SpinningLockMinigame spinningLockMinigame;

    [Header("Door Handle Objects")]
    [SerializeField] private GameObject lockedHandle;
    [SerializeField] private GameObject unlockedHandle;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Sprite patchPortrait;

    [Header("Opening Dialogue")]
    [SerializeField] private string speakerName = "Patch";

    [TextArea(3, 6)]
    [SerializeField]
    private string openingDialogue =
        "What's this? It looks like I have to unlock this first.";

    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Completion Dialogue")]
    [TextArea(3, 6)]
    [SerializeField]
    private string completionDialogue =
        "Yes! I've done it.";

    [Header("Player Movement Lock")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Animator playerAnimator;

    [Header("State")]
    [SerializeField] private bool lockCompleted = false;

    private bool dialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private bool showingCompletionDialogue = false;

    // InteractionDetector uses this to know that
    // E should continue being sent to this script.
    public bool IsDialogueActive => dialogueActive;

    public bool CanInteract()
    {
        // Before Patch has spoken to Devil Dog,
        // this lock cannot be used.
        if (!StoryProgress.HasTalkedToDevilDog)
            return false;

        // Once the lock is completed,
        // the locked handle is no longer interactable.
        return !lockCompleted;
    }

    public void Interact()
    {
        if (lockCompleted)
            return;

        // The lock cannot be examined until Patch
        // has completed his first Devil Dog conversation.
        if (!StoryProgress.HasTalkedToDevilDog)
            return;

        // If either the opening or completion dialogue
        // is already active, E advances that dialogue.
        if (dialogueActive)
        {
            HandleDialogueInput();
            return;
        }

        // Otherwise this is Patch's first interaction
        // with the available lock.
        StartOpeningDialogue();
    }

    // =========================================================
    // OPENING DIALOGUE
    // =========================================================

    private void StartOpeningDialogue()
    {
        dialogueActive = true;
        showingCompletionDialogue = false;

        FreezePlayer();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (nameText != null)
        {
            nameText.text = speakerName;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = patchPortrait;
            portraitImage.enabled = patchPortrait != null;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypeOpeningDialogue());
    }

    private IEnumerator TypeOpeningDialogue()
    {
        isTyping = true;

        if (dialogueText == null)
        {
            isTyping = false;
            typingCoroutine = null;
            yield break;
        }

        dialogueText.text = openingDialogue;
        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i <= openingDialogue.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }

        dialogueText.maxVisibleCharacters =
            openingDialogue.Length;

        isTyping = false;
        typingCoroutine = null;
    }

    // =========================================================
    // COMPLETION DIALOGUE
    // =========================================================

    // SpinningLockMinigame calls this when Patch reaches 3/3.
    public void OnMinigameCompleted()
    {
        showingCompletionDialogue = true;
        dialogueActive = true;

        // Patch should remain frozen while the completed
        // lock UI and dialogue are on screen.
        FreezePlayer();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (nameText != null)
        {
            nameText.text = speakerName;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = patchPortrait;
            portraitImage.enabled = patchPortrait != null;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypeCompletionDialogue());
    }

    private IEnumerator TypeCompletionDialogue()
    {
        isTyping = true;

        if (dialogueText == null)
        {
            isTyping = false;
            typingCoroutine = null;
            yield break;
        }

        dialogueText.text = completionDialogue;
        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i <= completionDialogue.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }

        dialogueText.maxVisibleCharacters =
            completionDialogue.Length;

        isTyping = false;
        typingCoroutine = null;
    }

    // =========================================================
    // DIALOGUE INPUT
    // =========================================================

    private void HandleDialogueInput()
    {
        // -----------------------------------------
        // COMPLETION DIALOGUE
        // -----------------------------------------

        if (showingCompletionDialogue)
        {
            // If it is still typing, E reveals
            // the whole completion sentence.
            if (isTyping)
            {
                CompleteCurrentDialogueImmediately(
                    completionDialogue
                );

                return;
            }

            // If the sentence is already visible,
            // E finishes the entire lock sequence.
            FinishLockSequence();
            return;
        }

        // -----------------------------------------
        // OPENING DIALOGUE
        // -----------------------------------------

        // If it is still typing, E reveals
        // the entire opening sentence.
        if (isTyping)
        {
            CompleteCurrentDialogueImmediately(
                openingDialogue
            );

            return;
        }

        // If the opening sentence is already visible,
        // E closes it and starts the minigame.
        FinishOpeningDialogue();
    }

    private void CompleteCurrentDialogueImmediately(
        string fullDialogue)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = fullDialogue;
            dialogueText.maxVisibleCharacters =
                fullDialogue.Length;
        }

        isTyping = false;
    }

    // =========================================================
    // OPEN MINIGAME
    // =========================================================

    private void FinishOpeningDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueActive = false;
        isTyping = false;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // IMPORTANT:
        //
        // Do not unfreeze Patch here.
        //
        // The minigame immediately takes over and keeps
        // Patch's movement/interactions locked.
        if (spinningLockMinigame != null)
        {
            spinningLockMinigame.StartMinigame();
        }
        else
        {
            Debug.LogWarning(
                "ClosetLockInteractable: SpinningLockMinigame is not assigned."
            );

            UnfreezePlayer();
        }
    }

    // =========================================================
    // FINISH LOCK
    // =========================================================

    private void FinishLockSequence()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueActive = false;
        isTyping = false;
        showingCompletionDialogue = false;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // NOW the story officially considers
        // the closet lock completed.
        StoryProgress.ClosetLockMinigameCompleted = true;

        lockCompleted = true;

        // Close the spinning-lock UI.
        if (spinningLockMinigame != null)
        {
            spinningLockMinigame.FinishSuccessfulMinigame();
        }

        // Show the unlocked handle BEFORE disabling
        // the locked handle.
        if (unlockedHandle != null)
        {
            unlockedHandle.SetActive(true);
        }

        // Restore Patch's movement before this GameObject
        // potentially disables itself.
        UnfreezePlayer();

        // This script is attached to the locked handle,
        // so this should be the final action.
        if (lockedHandle != null)
        {
            lockedHandle.SetActive(false);
        }
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