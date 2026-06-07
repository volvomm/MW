using TMPro;
using UnityEngine;

public class PuzzleDigitButton : MonoBehaviour
{
    public TextMeshProUGUI digitText;
    public PuzzleBoxManager puzzleManager;

    private int currentNumber = 0;

    public void IncreaseNumber()
    {
        currentNumber++;

        if (currentNumber > 9)
        {
            currentNumber = 0;
        }

        digitText.text = currentNumber.ToString();

        if (puzzleManager != null)
        {
            puzzleManager.CheckCode();
        }
    }

    public int GetNumber()
    {
        return currentNumber;
    }

    public void ResetNumber()
    {
        currentNumber = 0;
        digitText.text = "0";
    }
}