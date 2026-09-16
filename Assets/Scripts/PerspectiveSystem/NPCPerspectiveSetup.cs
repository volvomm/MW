using UnityEngine;

public class NPCPerspectiveSetup : MonoBehaviour
{
    [Header("Mood System")]
    public MoodManager moodManager;

    [Header("NPC Visual")]
    public SpriteRenderer targetSpriteRenderer;

    [Header("Mood-Based Starting Visuals")]
    public MoodPerceptionVisual[] moodVisuals;

    [Header("Fallback Visual")]
    [Tooltip("Used if no matching mood visual is found.")]
    public Sprite defaultSprite;

    private void Start()
    {
        ApplyCurrentMoodVisual();
    }

    public void ApplyCurrentMoodVisual()
    {
        if (targetSpriteRenderer == null)
        {
            Debug.LogWarning(
                "NPCPerspectiveSetup: No Target Sprite Renderer assigned."
            );
            return;
        }

        if (moodManager == null)
        {
            Debug.LogWarning(
                "NPCPerspectiveSetup: No Mood Manager assigned."
            );

            ApplyDefaultSprite();
            return;
        }

        if (moodVisuals != null)
        {
            for (int i = 0; i < moodVisuals.Length; i++)
            {
                MoodPerceptionVisual visual = moodVisuals[i];

                if (visual != null &&
                    visual.mood == moodManager.currentMood &&
                    visual.sprite != null)
                {
                    targetSpriteRenderer.sprite = visual.sprite;
                    return;
                }
            }
        }

        ApplyDefaultSprite();
    }

    private void ApplyDefaultSprite()
    {
        if (defaultSprite != null &&
            targetSpriteRenderer != null)
        {
            targetSpriteRenderer.sprite = defaultSprite;
        }
    }

    // Returns the dialogue portrait that matches Patch's current mood.
    public Sprite GetCurrentDialoguePortrait()
    {
        if (moodManager == null)
        {
            return null;
        }

        if (moodVisuals != null)
        {
            for (int i = 0; i < moodVisuals.Length; i++)
            {
                MoodPerceptionVisual visual = moodVisuals[i];

                if (visual != null &&
                    visual.mood == moodManager.currentMood &&
                    visual.dialoguePortrait != null)
                {
                    return visual.dialoguePortrait;
                }
            }
        }

        return null;
    }
}