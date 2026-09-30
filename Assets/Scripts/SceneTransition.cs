
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerTargetPosition;
    [SerializeField] private Transform cameraTargetPosition;
    [SerializeField] private Transform mainCamera;

    [Header("Lock State")]
    [SerializeField] private bool canTransition = true;

    [Header("Required Key")]
    [SerializeField] private InventoryItemData requiredKey;

    [Header("Tin Can Entrance")]
    [Tooltip("Enable ONLY for the slime-covered tin can entrance.")]
    [SerializeField] private bool useTinCanEntrance = false;

    [Tooltip("Patch must walk this far into the entrance before transitioning.")]
    [SerializeField] private float entranceDepth = 0.3f;

    [Tooltip("Enable if Patch enters the hole by moving left instead of right.")]
    [SerializeField] private bool enterFromRight = true;

    public InventoryItemData RequiredKey => requiredKey;
    public bool CanTransition => canTransition;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (requiredKey != null)
        {
            canTransition = false;
        }
    }

    public void SetTransitionEnabled(bool enabledState)
    {
        canTransition = enabledState;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryTransition(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Allows Patch to enter immediately after
        // unlocking the entrance while already
        // overlapping its trigger.
        if (useTinCanEntrance)
        {
            TryTransition(collision);
        }
    }

    private void TryTransition(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (!canTransition || isTransitioning)
            return;

        if (useTinCanEntrance)
        {
            // Require Patch to move sufficiently far
            // into the entrance, rather than teleporting
            // immediately when the towel is used.
            float entranceCenterX = transform.position.x;

            if (enterFromRight)
            {
                if (collision.transform.position.x >
                    entranceCenterX - entranceDepth)
                {
                    return;
                }
            }
            else
            {
                if (collision.transform.position.x <
                    entranceCenterX + entranceDepth)
                {
                    return;
                }
            }
        }

        isTransitioning = true;

        if (player != null && playerTargetPosition != null)
        {
            player.position = playerTargetPosition.position;
        }

        if (mainCamera != null && cameraTargetPosition != null)
        {
            Vector3 newCameraPosition = mainCamera.position;

            newCameraPosition.x = cameraTargetPosition.position.x;
            newCameraPosition.y = cameraTargetPosition.position.y;

            mainCamera.position = newCameraPosition;
        }

        Debug.Log("Transition completed: " + gameObject.name);

        isTransitioning = false;
    }
}
