
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "SampleScene";

    // Called when the player clicks START.
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Called when the player clicks EXIT.
    public void ExitGame()
    {
        Debug.Log("Exiting Patch...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
