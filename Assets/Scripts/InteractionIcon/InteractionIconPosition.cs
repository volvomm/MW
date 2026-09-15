using UnityEngine;

public class InteractionIconPosition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Transform interactionIcon;

    [Header("Icon Position")]
    [SerializeField] private float facingRightX = 0.5f;
    [SerializeField] private float facingLeftX = -0.5f;

    private void LateUpdate()
    {
        if (playerSprite == null || interactionIcon == null)
            return;

        Vector3 iconPosition = interactionIcon.localPosition;

        if (playerSprite.flipX)
        {
            // Patch is facing left
            iconPosition.x = facingLeftX;
        }
        else
        {
            // Patch is facing right
            iconPosition.x = facingRightX;
        }

        interactionIcon.localPosition = iconPosition;
    }
}