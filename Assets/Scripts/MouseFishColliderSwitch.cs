using UnityEngine;

public class NPCSpriteColliderSwitcher : MonoBehaviour
{
    [Header("NPC Sprite")]
    [SerializeField] private SpriteRenderer npcSpriteRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite fishSprite;
    [SerializeField] private Sprite mouseSprite;

    [Header("Physical Colliders")]
    [SerializeField] private Collider2D fishCollider;
    [SerializeField] private Collider2D mouseCollider;

    private Sprite lastSprite;

    private void Start()
    {
        UpdateCollider();
    }

    private void LateUpdate()
    {
        if (npcSpriteRenderer == null)
            return;

        // Only update when the sprite has actually changed.
        if (npcSpriteRenderer.sprite != lastSprite)
        {
            UpdateCollider();
        }
    }

    private void UpdateCollider()
    {
        if (npcSpriteRenderer == null)
            return;

        lastSprite = npcSpriteRenderer.sprite;

        if (npcSpriteRenderer.sprite == fishSprite)
        {
            if (fishCollider != null)
                fishCollider.enabled = true;

            if (mouseCollider != null)
                mouseCollider.enabled = false;
        }
        else if (npcSpriteRenderer.sprite == mouseSprite)
        {
            if (fishCollider != null)
                fishCollider.enabled = false;

            if (mouseCollider != null)
                mouseCollider.enabled = true;
        }
    }
}
