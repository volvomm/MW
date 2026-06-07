using UnityEngine;

public class PuzzleBoxOpener : MonoBehaviour, IInteractable
{
    public GameObject puzzleBoxPanel;
    public PuzzleBoxManager puzzleBoxManager;

    private bool puzzleOpen = false;

    public bool CanInteract()
    {
        if (puzzleBoxManager != null && puzzleBoxManager.IsUnlocked())
            return false;

        return true;
    }

    public void Interact()
    {
        if (puzzleBoxManager != null && puzzleBoxManager.IsUnlocked())
            return;

        if (puzzleBoxPanel == null)
            return;

        puzzleOpen = !puzzleOpen;
        puzzleBoxPanel.SetActive(puzzleOpen);
    }
}