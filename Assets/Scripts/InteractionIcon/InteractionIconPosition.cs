using UnityEngine;

public class InteractionIconPosition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Transform interactionIcon;

    [Header("Facing Right Icon Position")]
    [SerializeField] private Vector2 facingRightPosition = new Vector2(0.5f, 0.5f);

    [Header("Facing Left Icon Position")]
    [SerializeField] private Vector2 facingLeftPosition = new Vector2(-0.5f, 0.5f);

    private void LateUpdate()
    {
        if (playerSprite == null || interactionIcon == null)
            return;

        Vector3 iconPosition = interactionIcon.localPosition;

        if (playerSprite.flipX)
        {
            // Patch is facing left.
            iconPosition.x = facingLeftPosition.x;
            iconPosition.y = facingLeftPosition.y;
        }
        else
        {
            // Patch is facing right.
            iconPosition.x = facingRightPosition.x;
            iconPosition.y = facingRightPosition.y;
        }

        interactionIcon.localPosition = iconPosition;
    }
}