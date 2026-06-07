using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TrapdoorKeyUnlock : MonoBehaviour, IInteractable
{
    public InventoryItemData requiredKey;

    public GameObject lockedTrapdoor;
    public GameObject unlockedTrapdoor;
    public GameObject basementTrigger;

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image portraitImage;
    public Sprite patchPortrait;

    public float typingSpeed = 0.03f;

    private bool isUnlocked = false;
    private bool dialogueActive = false;
    private bool isTyping = false;
    private bool canUseEForDialogue = false;
    private string currentLine = "";

    private void Update()
    {
        if (!dialogueActive)
            return;

        if (!canUseEForDialogue)
        {
            if (Input.GetKeyUp(KeyCode.E))
                canUseEForDialogue = true;

            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.SetText(currentLine);
                isTyping = false;
            }
            else
            {
                EndDialogue();
            }
        }
    }

    public bool CanInteract()
    {
        return !dialogueActive && !isUnlocked;
    }

    public void Interact()
    {
        if (dialogueActive)
            return;

        if (InventorySystem.Instance != null && InventorySystem.Instance.HasItem(requiredKey))
        {
            UnlockTrapdoor();
        }
        else
        {
            StartDialogue("Patch", "Oh, it's locked. I need something to unlock this.");
        }
    }

    private void UnlockTrapdoor()
    {
        isUnlocked = true;

        SpriteRenderer lockedSprite = lockedTrapdoor.GetComponent<SpriteRenderer>();
        if (lockedSprite != null)
            lockedSprite.enabled = false;

        Collider2D lockedCollider = lockedTrapdoor.GetComponent<Collider2D>();
        if (lockedCollider != null)
            lockedCollider.enabled = false;

        unlockedTrapdoor.SetActive(true);

        if (basementTrigger != null)
        {
            basementTrigger.SetActive(true);
        }

        InventorySystem.Instance.RemoveItem(requiredKey);

        StartDialogue("Patch", "The door's opened!");
    }

    private void StartDialogue(string speakerName, string line)
    {
        dialogueActive = true;
        isTyping = false;
        canUseEForDialogue = false;
        currentLine = line;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (nameText != null)
            nameText.SetText(speakerName);

        if (portraitImage != null && patchPortrait != null)
            portraitImage.sprite = patchPortrait;

        StopAllCoroutines();
        StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;

        if (dialogueText != null)
            dialogueText.SetText("");

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        isTyping = false;
        canUseEForDialogue = false;
        currentLine = "";

        if (dialogueText != null)
            dialogueText.SetText("");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}