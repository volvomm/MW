using UnityEngine;

public class DevilDogWalkOff : MonoBehaviour
{
    [Header("Walk Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private Transform exitPoint;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Player Movement Lock")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Animator playerAnimator;

    private bool isWalkingAway = false;

    private void Update()
    {
        if (!isWalkingAway)
        {
            return;
        }

        // Move Devil Dog to the left.
        transform.position +=
            Vector3.left * walkSpeed * Time.deltaTime;

        // Once Devil Dog reaches the exit point,
        // finish the sequence.
        if (exitPoint != null &&
            transform.position.x <= exitPoint.position.x)
        {
            FinishWalkingAway();
        }
    }

    public void StartWalkingAway()
    {
        if (isWalkingAway)
        {
            return;
        }

        isWalkingAway = true;

        // Freeze Patch once at the start.
        FreezePlayer();

        if (animator != null)
        {
            animator.enabled = true;

            animator.Play(
                "DevilDogWalk",
                0,
                0f
            );
        }
    }

    private void FinishWalkingAway()
    {
        isWalkingAway = false;

        // IMPORTANT:
        // Restore Patch's movement BEFORE disabling Devil Dog.
        UnfreezePlayer();

        // Devil Dog disappears from this room.
        gameObject.SetActive(false);
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
            playerAnimator.SetFloat("Speed", 0f);
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
            playerAnimator.SetFloat("Speed", 0f);
        }

        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
            playerMovement.enabled = true;
        }
    }
}