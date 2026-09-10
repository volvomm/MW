using UnityEngine;

public class MoodResetTrigger : MonoBehaviour
{
    [Header("Mood System")]
    public MoodManager moodManager;

    [Header("Mood To Set")]
    public PatchMood moodToSet = PatchMood.None;

    [Header("Trigger Settings")]
    public bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (triggerOnlyOnce && hasTriggered)
        {
            return;
        }

        if (moodManager == null)
        {
            Debug.LogWarning("MoodResetTrigger: Mood Manager has not been assigned.");
            return;
        }

        moodManager.SetMood(moodToSet);

        hasTriggered = true;
    }
}