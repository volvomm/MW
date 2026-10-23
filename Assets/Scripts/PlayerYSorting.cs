
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerYSorting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform sortingPoint;
    [SerializeField] private Transform trapdoorReference;

    [Header("Sorting Settings")]
    [SerializeField] private int normalSortingOrder = 6;
    [SerializeField] private int behindSortingOrder = 5;
    [SerializeField] private int frontSortingOrder = 7;

    [Header("Trapdoor Detection")]
    [SerializeField] private float sortingDistance = 4f;

    [Header("Trapdoor Side Detection")]
    [SerializeField] private float trapdoorHalfWidth = 2.5f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (sortingPoint == null)
            return;

        if (trapdoorReference == null ||
            !trapdoorReference.gameObject.activeInHierarchy)
        {
            spriteRenderer.sortingOrder = normalSortingOrder;
            return;
        }

        Vector2 playerPosition = sortingPoint.position;
        Vector2 trapdoorPosition = trapdoorReference.position;

        float distance = Vector2.Distance(
            playerPosition,
            trapdoorPosition
        );

        if (distance > sortingDistance)
        {
            spriteRenderer.sortingOrder = normalSortingOrder;
            return;
        }

        // Check whether Patch is outside the
        // central horizontal area of the trapdoor.
        float horizontalDistance = Mathf.Abs(
            playerPosition.x - trapdoorPosition.x
        );

        if (horizontalDistance > trapdoorHalfWidth)
        {
            spriteRenderer.sortingOrder = frontSortingOrder;
            return;
        }

        // Inside the central area, use Y sorting.
        if (playerPosition.y > trapdoorPosition.y)
        {
            spriteRenderer.sortingOrder = behindSortingOrder;
        }
        else
        {
            spriteRenderer.sortingOrder = frontSortingOrder;
        }
    }
}
