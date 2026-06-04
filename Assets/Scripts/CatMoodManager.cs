using TMPro;
using UnityEngine;

public class CatMoodManager : MonoBehaviour
{
    public static CatMoodManager Instance;

    public TextMeshProUGUI moodText;

    public string currentMood = "";

    private void Awake()
    {
        Instance = this;
        UpdateMoodUI();
    }

    public void SetMood(string mood)
    {
        currentMood = mood;
        UpdateMoodUI();
    }

    public void ClearMood()
    {
        currentMood = "";
        UpdateMoodUI();
    }

    private void UpdateMoodUI()
    {
        if (moodText == null)
        {
            return;
        }

        if (currentMood == "")
        {
            moodText.text = "Mood:";
        }
        else
        {
            moodText.text = "Mood: " + currentMood;
        }
    }
}
