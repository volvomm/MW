using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class SpinningLockMinigame : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private GameObject minigameUI;

    [Header("Player Lock")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private InteractionDetector interactionDetector;

    [Header("Rotating Indicator")]
    [SerializeField] private RectTransform indicatorPivot;
    [SerializeField] private RectTransform spinningCircle;

    [Header("Red Targets")]
    [SerializeField] private GameObject redTarget1;
    [SerializeField] private GameObject redTarget2;
    [SerializeField] private GameObject redTarget3;

    [Header("Hit Zones")]
    [SerializeField] private RectTransform hitZone1;
    [SerializeField] private RectTransform hitZone2;
    [SerializeField] private RectTransform hitZone3;

    [Header("Success Counter")]
    [SerializeField] private TMP_Text successCounterText;

    [Header("Completion")]
    [SerializeField] private ClosetLockInteractable closetLockInteractable;

    [Header("Instruction Text")]
    [SerializeField] private TMP_Text instructionText;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float startingRotation = 176f;

    private int successes = 0;
    private bool minigameActive = false;
    private bool minigameCompleted = false;

    // NEW:
    // Becomes true while Patch's
    // "Yes! I've done it." dialogue is on screen.
    private bool waitingForCompletionDialogue = false;

    private void Start()
    {
        // The minigame starts hidden.
        if (minigameUI != null)
        {
            minigameUI.SetActive(false);
        }
    }

    private void Update()
    {
        // =====================================================
        // COMPLETION DIALOGUE INPUT
        // =====================================================

        // Once the minigame reaches 3/3, normal world
        // interactions are still locked.
        //
        // Therefore THIS script temporarily owns the E key
        // and sends it directly to ClosetLockInteractable.
        if (waitingForCompletionDialogue)
        {
            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (closetLockInteractable != null)
                {
                    closetLockInteractable.Interact();
                }
            }

            return;
        }

        // =====================================================
        // NORMAL MINIGAME
        // =====================================================

        if (!minigameActive)
        {
            return;
        }

        // Rotate the black circle around the white ring.
        if (indicatorPivot != null)
        {
            indicatorPivot.Rotate(
                0f,
                0f,
                -rotationSpeed * Time.deltaTime
            );
        }

        // E attempts to land the black circle
        // on the currently active red target.
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            AttemptHit();
        }
    }

    public void StartMinigame()
    {
        // Every new attempt begins at 0/3.
        successes = 0;
        minigameActive = true;
        waitingForCompletionDialogue = false;

        // Lock Patch before showing the minigame.
        LockPlayer();

        // Show the close-up lock UI.
        if (minigameUI != null)
        {
            minigameUI.SetActive(true);
        }

        // Restore the normal instruction.
        if (instructionText != null)
        {
            instructionText.text =
                "Press E to land on the red bar";
        }

        // Reset the black circle to the starting position.
        if (indicatorPivot != null)
        {
            indicatorPivot.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    startingRotation
                );
        }

        // Show RedTarget1.
        ShowCorrectRedTarget();

        // Display 0/3.
        UpdateCounter();
    }

    private void AttemptHit()
    {
        RectTransform currentHitZone =
            GetCurrentHitZone();

        if (currentHitZone == null ||
            spinningCircle == null)
        {
            return;
        }

        // Get the centre position of the black circle.
        Vector2 screenPoint =
            RectTransformUtility.WorldToScreenPoint(
                null,
                spinningCircle.position
            );

        // Check whether the centre of the black circle
        // is inside the invisible hit zone.
        bool landedOnRed =
            RectTransformUtility.RectangleContainsScreenPoint(
                currentHitZone,
                screenPoint,
                null
            );

        if (landedOnRed)
        {
            SuccessfulHit();
        }
        else
        {
            FailedHit();
        }
    }

    private RectTransform GetCurrentHitZone()
    {
        // At 0/3 we are aiming for RedTarget1.
        if (successes == 0)
        {
            return hitZone1;
        }

        // At 1/3 we are aiming for RedTarget2.
        if (successes == 1)
        {
            return hitZone2;
        }

        // At 2/3 we are aiming for RedTarget3.
        if (successes == 2)
        {
            return hitZone3;
        }

        return null;
    }

    private void SuccessfulHit()
    {
        successes++;

        UpdateCounter();

        // Three successful hits completes the minigame.
        if (successes >= 3)
        {
            CompleteMinigame();
            return;
        }

        // Otherwise move to the next red target.
        ShowCorrectRedTarget();
    }

    private void FailedHit()
    {
        // If we're already at 0/3,
        // a failure cannot reduce the counter further.
        if (successes <= 0)
        {
            successes = 0;
            UpdateCounter();
            return;
        }

        // Remove one successful hit.
        successes--;

        UpdateCounter();

        // 1/3 -> failure -> 0/3
        // Close the minigame.
        if (successes == 0)
        {
            CloseMinigame();
            return;
        }

        // 2/3 -> failure -> 1/3
        ShowCorrectRedTarget();
    }

    private void CompleteMinigame()
    {
        // Stop the spinning gameplay.
        minigameActive = false;

        // Prevent the minigame from being restarted.
        minigameCompleted = true;

        // IMPORTANT:
        //
        // We are now waiting for Patch's completion
        // dialogue to be finished with E.
        waitingForCompletionDialogue = true;

        // Show completion text.
        if (instructionText != null)
        {
            instructionText.text = "Unlocked!";
        }

        // Hide all red targets.
        if (redTarget1 != null)
        {
            redTarget1.SetActive(false);
        }

        if (redTarget2 != null)
        {
            redTarget2.SetActive(false);
        }

        if (redTarget3 != null)
        {
            redTarget3.SetActive(false);
        }

        // Keep Patch locked.
        //
        // InteractionDetector also remains locked so E
        // cannot accidentally interact with anything
        // behind the minigame UI.

        // Tell the closet lock to display:
        //
        // "Yes! I've done it."
        if (closetLockInteractable != null)
        {
            closetLockInteractable.OnMinigameCompleted();
        }
        else
        {
            Debug.LogWarning(
                "SpinningLockMinigame: ClosetLockInteractable is not assigned."
            );
        }
    }

    private void ShowCorrectRedTarget()
    {
        if (redTarget1 != null)
        {
            redTarget1.SetActive(successes == 0);
        }

        if (redTarget2 != null)
        {
            redTarget2.SetActive(successes == 1);
        }

        if (redTarget3 != null)
        {
            redTarget3.SetActive(successes == 2);
        }
    }

    private void CloseMinigame()
    {
        minigameActive = false;
        waitingForCompletionDialogue = false;

        // Hide the close-up lock UI.
        if (minigameUI != null)
        {
            minigameUI.SetActive(false);
        }

        // Give Patch his controls back.
        UnlockPlayer();
    }

    public void FinishSuccessfulMinigame()
    {
        minigameActive = false;
        waitingForCompletionDialogue = false;

        // Hide the completed lock UI.
        if (minigameUI != null)
        {
            minigameUI.SetActive(false);
        }

        // Give Patch his controls back.
        UnlockPlayer();
    }

    private void UpdateCounter()
    {
        if (successCounterText != null)
        {
            successCounterText.text =
                successes + "/3";
        }
    }

    private void LockPlayer()
    {
        // Immediately stop Patch and prevent movement.
        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
            playerMovement.enabled = false;
        }

        // Prevent E from activating normal world
        // interactables behind the minigame.
        if (interactionDetector != null)
        {
            interactionDetector.interactionsLocked = true;

            // Hide the normal E interaction icon.
            if (interactionDetector.interactionIcon != null)
            {
                interactionDetector.interactionIcon.SetActive(false);
            }
        }
    }

    private void UnlockPlayer()
    {
        // Restore Patch's movement.
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Restore normal world interactions.
        if (interactionDetector != null)
        {
            interactionDetector.interactionsLocked = false;
        }
    }
}