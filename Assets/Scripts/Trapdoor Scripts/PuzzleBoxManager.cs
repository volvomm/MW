using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PuzzleBoxManager : MonoBehaviour
{
    [Header("Digit Buttons")]
    public PuzzleDigitButton digit1, digit2, digit3, digit4;

    [Header("Disable After Solved")]
    public GameObject puzzleBoxInteractObject;

    [Header("Correct Code")]
    public int correctDigit1 = 1, correctDigit2 = 2, correctDigit3 = 3, correctDigit4 = 4;

    [Header("Reward")]
    public InventoryItemData basementKeyItem;

    [Header("UI")]
    public GameObject puzzlePanel;

    [Header("Dialogue")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image portraitImage;
    public Sprite patchPortrait;
    public float textSpeed = 0.04f;

    private bool unlocked = false;
    private bool dialogueActive = false;
    private bool isTyping = false;
    private int dialogueIndex = 0;

    private string[] solvedLines =
    {
        "The box opened!",
        "Oh, there's another key in here."
    };

    void Update()
    {
        if (!dialogueActive)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = solvedLines[dialogueIndex];
                isTyping = false;
            }
            else
            {
                dialogueIndex++;

                if (dialogueIndex < solvedLines.Length)
                {
                    StartCoroutine(TypeDialogue(solvedLines[dialogueIndex]));
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    public bool CheckCode()
    {
        if (unlocked) return true;

        bool correct =
            digit1.GetNumber() == correctDigit1 &&
            digit2.GetNumber() == correctDigit2 &&
            digit3.GetNumber() == correctDigit3 &&
            digit4.GetNumber() == correctDigit4;

        if (correct)
        {
            unlocked = true;

            if (puzzlePanel != null)
                puzzlePanel.SetActive(false);

            if (InventorySystem.Instance != null && basementKeyItem != null)
                InventorySystem.Instance.AddItem(basementKeyItem);

            StartSolvedDialogue();
        }

        return correct;
    }

    private void StartSolvedDialogue()
    {
        dialogueActive = true;
        dialogueIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (nameText != null)
            nameText.text = "Patch";

        if (portraitImage != null && patchPortrait != null)
            portraitImage.sprite = patchPortrait;

        StartCoroutine(TypeDialogue(solvedLines[dialogueIndex]));
    }

    private IEnumerator TypeDialogue(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }
    private void EndDialogue()
    {
        dialogueActive = false;
        dialogueIndex = 0;

        if (dialogueText != null)
            dialogueText.text = "";

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (puzzleBoxInteractObject != null)
            puzzleBoxInteractObject.SetActive(false);
    }

    public bool IsUnlocked()
    {
        return unlocked;
    }
}