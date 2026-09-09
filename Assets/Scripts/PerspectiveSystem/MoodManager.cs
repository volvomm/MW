using UnityEngine;
using TMPro;

public class MoodManager : MonoBehaviour
{
    [Header("Current Mood")]
    public PatchMood currentMood = PatchMood.None;

    [Header("Mood UI")]
    public TextMeshProUGUI moodText;

    private void Start()
    {
        UpdateMoodUI();
    }

    private void Update()
    {
        UpdateMoodUI();
    }

    public void SetMood(PatchMood newMood)
    {
        currentMood = newMood;
        UpdateMoodUI();
    }

    public void ClearMood()
    {
        SetMood(PatchMood.None);
    }

    private void UpdateMoodUI()
    {
        if (moodText == null)
        {
            Debug.LogWarning("MoodManager: No Mood Text has been assigned.");
            return;
        }

        if (currentMood == PatchMood.None)
        {
            moodText.text = "Mood:";
        }
        else
        {
            moodText.text = "Mood: " + currentMood.ToString();
        }
    }
}