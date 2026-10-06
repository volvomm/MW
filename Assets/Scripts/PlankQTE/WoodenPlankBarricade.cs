using System.Collections;
using UnityEngine;

public class WoodenPlankBarricade : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private InventoryItemData woodenPlankItem;

    [Header("Plank QTE")]
    [SerializeField] private PlankQTEManager plankQTEManager;

    [Header("Placed Plank Visuals")]
    [SerializeField] private GameObject woodLean1;
    [SerializeField] private GameObject woodLean2;
    [SerializeField] private GameObject woodLean3;

    [Header("Completed Barricade Collider")]
    [SerializeField] private GameObject closetBarricadeBlocker;

    [Header("Barricade Finished Dialogue")]
    [SerializeField] private DoorQuestionTransition doorQuestionTransition;

    [Header("Patch Placement Movement")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform plankPlacementPatchPosition;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private float autoWalkSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.05f;

    private bool qteInProgress;
    private bool autoWalking;

    private void Start()
    {
        // These are revealed one at a time when Patch
        // successfully places each plank.
        if (woodLean1 != null)
            woodLean1.SetActive(false);

        if (woodLean2 != null)
            woodLean2.SetActive(false);

        if (woodLean3 != null)
            woodLean3.SetActive(false);

        if (closetBarricadeBlocker != null)
            closetBarricadeBlocker.SetActive(false);
    }

    public bool CanPlacePlank()
    {
        // Devil Dog must already be trapped in the closet.
        if (!StoryProgress.DevilDogTrapdoorSequenceFinished)
            return false;

        // All three planks have already been placed.
        if (StoryProgress.ClosetPlanksPlaced >= 3)
            return false;

        // Don't start another placement while Patch
        // is already moving or doing the QTE.
        if (qteInProgress || autoWalking)
            return false;

        // Patch needs to actually be carrying a wooden plank.
        if (InventorySystem.Instance == null ||
            woodenPlankItem == null)
        {
            return false;
        }

        return InventorySystem.Instance.HasItem(woodenPlankItem);
    }

    public void StartPlankPlacement()
    {
        if (!CanPlacePlank())
            return;

        if (plankQTEManager == null)
        {
            Debug.LogWarning(
                "WoodenPlankBarricade: Plank QTE Manager has not been assigned."
            );
            return;
        }

        if (player == null ||
            plankPlacementPatchPosition == null ||
            playerMovement == null)
        {
            Debug.LogWarning(
                "WoodenPlankBarricade: Patch movement references have not been assigned."
            );
            return;
        }

        qteInProgress = true;

        StartCoroutine(MovePatchIntoPlacementPosition());
    }

    private IEnumerator MovePatchIntoPlacementPosition()
    {
        autoWalking = true;

        // Stop manual player control.
        playerMovement.StopMovementImmediately();
        playerMovement.enabled = false;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        // -------------------------------------------------
        // SET FACING DIRECTION BEFORE AUTO-WALKING
        // -------------------------------------------------
        //
        // WoodLean1 = RIGHT
        // WoodLean2 = LEFT
        // WoodLean3 = RIGHT
        //
        if (playerMovement.rend != null)
        {
            if (StoryProgress.ClosetPlanksPlaced == 1)
            {
                // About to place WoodLean2.
                playerMovement.rend.flipX = true;
            }
            else
            {
                // About to place WoodLean1 or WoodLean3.
                playerMovement.rend.flipX = false;
            }
        }

        // Keep moving until Patch reaches the placement point.
        while (Vector2.Distance(
                   player.position,
                   plankPlacementPatchPosition.position)
               > stoppingDistance)
        {
            Vector2 currentPosition = player.position;

            Vector2 targetPosition =
                plankPlacementPatchPosition.position;

            Vector2 direction =
                (targetPosition - currentPosition).normalized;

            // Play Patch's walking animation.
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("Speed", 1f);
            }

            player.position = Vector2.MoveTowards(
                player.position,
                plankPlacementPatchPosition.position,
                autoWalkSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Snap Patch exactly onto the placement position.
        player.position =
            plankPlacementPatchPosition.position;

        // Stop walking animation.
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        // -------------------------------------------------
        // FACE THE CORRECT DIRECTION FOR EACH PLANK
        // -------------------------------------------------
        //
        // ClosetPlanksPlaced tells us how many planks are
        // already on the closet:
        //
        // 0 = about to place WoodLean1 -> face RIGHT
        // 1 = about to place WoodLean2 -> face LEFT
        // 2 = about to place WoodLean3 -> face RIGHT
        //
        if (playerMovement != null &&
            playerMovement.rend != null)
        {
            if (StoryProgress.ClosetPlanksPlaced == 1)
            {
                // WoodLean2
                playerMovement.rend.flipX = true;
            }
            else
            {
                // WoodLean1 and WoodLean3
                playerMovement.rend.flipX = false;
            }
        }

        autoWalking = false;

        // NOW begin the QTE.
        plankQTEManager.StartBarricadeQTE(this);
    }

    public void QTESucceeded()
    {
        qteInProgress = false;

        if (InventorySystem.Instance == null ||
            woodenPlankItem == null)
        {
            RestorePlayerMovement();
            return;
        }

        // Make sure Patch still has the plank.
        if (!InventorySystem.Instance.HasItem(woodenPlankItem))
        {
            RestorePlayerMovement();
            return;
        }

        // Remove the carried plank.
        InventorySystem.Instance.RemoveItem(woodenPlankItem);

        InventoryUIController inventoryUI =
            FindFirstObjectByType<InventoryUIController>();

        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        // Increase the number of planks actually attached
        // to the closet.
        StoryProgress.ClosetPlanksPlaced++;

        // -------------------------------------------------
        // WOODLEAN 1
        // -------------------------------------------------
        if (StoryProgress.ClosetPlanksPlaced == 1)
        {
            if (woodLean1 != null)
            {
                woodLean1.SetActive(true);
            }
        }

        // -------------------------------------------------
        // WOODLEAN 2
        // -------------------------------------------------
        else if (StoryProgress.ClosetPlanksPlaced == 2)
        {
            if (woodLean2 != null)
            {
                woodLean2.SetActive(true);
            }
        }

        // -------------------------------------------------
        // WOODLEAN 3 - BARRICADE FINISHED
        // -------------------------------------------------
        else if (StoryProgress.ClosetPlanksPlaced == 3)
        {
            if (woodLean3 != null)
            {
                woodLean3.SetActive(true);
            }

            // All three planks are now in place.
            if (closetBarricadeBlocker != null)
            {
                closetBarricadeBlocker.SetActive(true);
            }

            StoryProgress.ClosetBarricaded = true;

            // Patch says:
            // "Now it's time to reunite the mother with her kittens."
            if (doorQuestionTransition != null)
            {
                doorQuestionTransition.PlayBarricadeFinishedDialogue();
            }
        }

        // -------------------------------------------------
        // RESTORE MOVEMENT
        // -------------------------------------------------
        //
        // After WoodLean1 and WoodLean2, Patch can move
        // again immediately.
        //
        // After WoodLean3, DON'T restore movement here.
        // DoorQuestionTransition has just started Patch's
        // dialogue and will restore his movement when the
        // dialogue is closed.
        //
        if (StoryProgress.ClosetPlanksPlaced < 3)
        {
            RestorePlayerMovement();
        }

        Debug.Log(
            "Closet plank placed. Total: " +
            StoryProgress.ClosetPlanksPlaced + "/3"
        );
    }

    public void QTEFailed()
    {
        qteInProgress = false;

        RestorePlayerMovement();

        Debug.Log(
            "Closet plank placement QTE failed. Patch can retry."
        );
    }

    private void RestorePlayerMovement()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
        }

        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
            playerMovement.enabled = true;
        }
    }
}