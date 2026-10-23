
using UnityEngine;

public class NPCPerspectiveSetup : MonoBehaviour
{
    [Header("Mood System")]
    public MoodManager moodManager;

    [Header("NPC Visual")]
    public SpriteRenderer targetSpriteRenderer;

    [Header("Animation (Optional)")]
    [SerializeField] private Animator animator;

    [Header("Mood-Based Starting Visuals")]
    public MoodPerceptionVisual[] moodVisuals;

    [Header("Fallback Visual")]
    [Tooltip("Used if no matching mood visual is found.")]
    public Sprite defaultSprite;

    private PatchMood lastMood;

    private void Start()
    {
        if (moodManager != null)
        {
            lastMood = moodManager.currentMood;
        }

        ApplyCurrentMoodVisual();
    }

    private void Update()
    {
        if (moodManager == null)
        {
            return;
        }

        if (moodManager.currentMood != lastMood)
        {
            lastMood = moodManager.currentMood;
            ApplyCurrentMoodVisual();
        }
    }

    public void ApplyCurrentMoodVisual()
    {
        if (moodManager == null)
        {
            ApplyDefaultSprite();
            return;
        }

        // If an Animator is assigned, use the correct
        // idle animation instead of directly changing
        // the Sprite Renderer.
        if (animator != null)
        {
            // Do not interrupt the walking-away sequence.
            DevilDogWalkOff walkOff =
                GetComponent<DevilDogWalkOff>();

            if (walkOff != null &&
                walkOff.IsWalkingAway)
            {
                return;
            }

            string idleState =
                moodManager.currentMood == PatchMood.Frightened
                ? "DevilDogIdle"
                : "NeutralDogIdle";

            animator.Play(idleState, 0, 0f);
            return;
        }

        // Preserve sprite-based visuals for NPCs
        // that do not use an Animator.
        if (targetSpriteRenderer == null)
        {
            return;
        }

        foreach (MoodPerceptionVisual visual in moodVisuals)
        {
            if (visual != null &&
                visual.mood == moodManager.currentMood &&
                visual.sprite != null)
            {
                targetSpriteRenderer.sprite = visual.sprite;
                return;
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

    public Sprite GetCurrentDialoguePortrait()
    {
        if (moodManager == null)
        {
            return null;
        }

        if (moodVisuals != null)
        {
            foreach (MoodPerceptionVisual visual in moodVisuals)
            {
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
