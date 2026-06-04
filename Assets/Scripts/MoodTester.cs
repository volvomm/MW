using UnityEngine;

public class MoodTester : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            CatMoodManager.Instance.SetMood("Frightened");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CatMoodManager.Instance.ClearMood();
        }
    }
}