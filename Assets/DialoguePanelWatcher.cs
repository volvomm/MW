using UnityEngine;

public class DialoguePanelWatcher : MonoBehaviour
{
    private void OnDisable()
    {
        Debug.LogWarning(
            "DialoguePanel was disabled!\n" +
            System.Environment.StackTrace
        );
    }
}