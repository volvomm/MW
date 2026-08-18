using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DoorQuestionTransition : MonoBehaviour
{
    [Header("Door Question UI")]
    public GameObject questionPanel;
    public TMP_Text questionText;
    public Button yesButton;
    public Button noButton;
    public CanvasGroup fadePanel;

    [Header("Transition Settings")]
    public Transform player;
    public Transform cameraPoint;
    public Transform spawnPoint;
    public Camera mainCamera;

    [Header("Settings")]
    public float fadeSpeed = 1f;
    public string question = "Enter the closet?";

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private Sprite patchPortrait;
    [SerializeField] private GameObject closeButton;

    [Header("Early Locked Dialogue")]
    [SerializeField] private string lockedSpeakerName = "Patch";

    [TextArea(3, 6)]
    [SerializeField]
    private string lockedDialogue =
        "I think I see something in the other room.";

    [Header("Closet Barricade")]
    [SerializeField] private InventoryItemData woodenPlankItem;
    [SerializeField] private GameObject normalClosetDoor;
    [SerializeField] private GameObject lockedClosetDoor;

    [TextArea(3, 6)]
    [SerializeField]
    private string barricadeDialogue =
        "Now it's time to reunite the mother with her kittens.";

    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Player Movement")]
    [SerializeField] private PlayerMovement playerMovementScript;

    private bool playerInside;
    private bool questionOpen;

    private bool patchDialogueActive;
    private bool isTyping;

    private bool closetBarricaded;

    private Coroutine typingCoroutine;

    private string currentDialogue;

    private void Start()
    {
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
        }

        // Normal door starts visible.
        if (normalClosetDoor != null)
        {
            normalClosetDoor.SetActive(true);
        }

        // Barricaded version starts hidden.
        if (lockedClosetDoor != null)
        {
            lockedClosetDoor.SetActive(false);
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        // If Patch dialogue is already on screen,
        // E handles that dialogue instead of interacting
        // with the door again.
        if (patchDialogueActive)
        {
            HandlePatchDialogueInput();
            return;
        }

        if (!playerInside || questionOpen)
        {
            return;
        }

        // ------------------------------------------------
        // AFTER DEVIL DOG DISAPPEARS INTO THE CLOSET
        // ------------------------------------------------
        if (StoryProgress.DevilDogTrapdoorSequenceFinished)
        {
            TryBarricadeCloset();
            return;
        }

        // ------------------------------------------------
        // EARLIER STORY BEHAVIOUR
        // ------------------------------------------------

        // Before Patch talks to the Devil Dog,
        // don't allow the closet question.
        if (!StoryProgress.HasTalkedToDevilDog)
        {
            StartPatchDialogue(lockedDialogue);
            return;
        }

        // After talking to Devil Dog but BEFORE
        // the final trapdoor sequence, the closet still
        // works normally.
        OpenQuestion();
    }

    private void TryBarricadeCloset()
    {
        // Already finished. Don't do anything again.
        if (closetBarricaded)
        {
            return;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning(
                "DoorQuestionTransition: InventorySystem.Instance was not found."
            );

            return;
        }

        // Patch MUST have the wooden plank.
        if (woodenPlankItem == null ||
            !InventorySystem.Instance.HasItem(woodenPlankItem))
        {
            // No dialogue and no door question.
            // Patch simply needs to collect the plank first.
            return;
        }

        BarricadeCloset();
    }

    private void BarricadeCloset()
    {
        closetBarricaded = true;
        StoryProgress.ClosetBarricaded = true;

        // Stop Patch immediately.
        SetPlayerMovement(false);

        // Remove wooden plank from inventory.
        InventorySystem.Instance.RemoveItem(woodenPlankItem);

        // Refresh inventory display.
        InventoryUIController inventoryUI =
            FindFirstObjectByType<InventoryUIController>();

        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        // Hide normal closet door.
        if (normalClosetDoor != null)
        {
            normalClosetDoor.SetActive(false);
        }

        // Show barricaded closet door.
        if (lockedClosetDoor != null)
        {
            lockedClosetDoor.SetActive(true);
        }

        // Only AFTER the door changes,
        // Patch says the reunion line.
        StartPatchDialogue(barricadeDialogue);
    }

    private void StartPatchDialogue(string sentence)
    {
        patchDialogueActive = true;
        currentDialogue = sentence;

        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (speakerNameText != null)
        {
            speakerNameText.text = lockedSpeakerName;
        }

        if (speakerPortraitImage != null)
        {
            speakerPortraitImage.sprite = patchPortrait;
            speakerPortraitImage.enabled =
                patchPortrait != null;
        }

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        SetPlayerMovement(false);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypePatchDialogue());
    }

    private IEnumerator TypePatchDialogue()
    {
        isTyping = true;

        if (dialogueText == null)
        {
            isTyping = false;
            typingCoroutine = null;
            yield break;
        }

        dialogueText.text = currentDialogue;
        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i <= currentDialogue.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i;

            yield return new WaitForSeconds(typingSpeed);
        }

        dialogueText.maxVisibleCharacters =
            currentDialogue.Length;

        isTyping = false;
        typingCoroutine = null;
    }

    private void HandlePatchDialogueInput()
    {
        // First E while typing:
        // immediately reveal the complete sentence.
        if (isTyping)
        {
            CompletePatchDialogueImmediately();
            return;
        }

        // Next E:
        // close the dialogue.
        ClosePatchDialogue();
    }

    private void CompletePatchDialogueImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = currentDialogue;
            dialogueText.maxVisibleCharacters =
                currentDialogue.Length;
        }

        isTyping = false;
    }

    private void ClosePatchDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        patchDialogueActive = false;
        isTyping = false;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.SetActive(true);
        }

        SetPlayerMovement(true);
    }

    private void SetPlayerMovement(bool canMove)
    {
        if (playerMovementScript == null)
        {
            return;
        }

        if (!canMove)
        {
            playerMovementScript.StopMovementImmediately();
            playerMovementScript.enabled = false;
        }
        else
        {
            playerMovementScript.StopMovementImmediately();
            playerMovementScript.enabled = true;
        }
    }

    private void OpenQuestion()
    {
        questionOpen = true;

        if (questionPanel != null)
        {
            questionPanel.SetActive(true);
        }

        if (questionText != null)
        {
            questionText.text = question;
        }

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(YesPressed);
        noButton.onClick.AddListener(NoPressed);
    }

    private void YesPressed()
    {
        StartCoroutine(FadeAndMove());
    }

    private void NoPressed()
    {
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        questionOpen = false;
    }

    private IEnumerator FadeAndMove()
    {
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        yield return StartCoroutine(Fade(1f));

        if (player != null && spawnPoint != null)
        {
            player.position = spawnPoint.position;
        }

        if (mainCamera != null && cameraPoint != null)
        {
            mainCamera.transform.position = new Vector3(
                cameraPoint.position.x,
                cameraPoint.position.y,
                mainCamera.transform.position.z
            );
        }

        yield return StartCoroutine(Fade(0f));

        questionOpen = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadePanel == null)
        {
            yield break;
        }

        while (!Mathf.Approximately(
                   fadePanel.alpha,
                   targetAlpha))
        {
            fadePanel.alpha = Mathf.MoveTowards(
                fadePanel.alpha,
                targetAlpha,
                fadeSpeed * Time.deltaTime
            );

            yield return null;
        }

        fadePanel.alpha = targetAlpha;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;

        if (questionOpen)
        {
            if (questionPanel != null)
            {
                questionPanel.SetActive(false);
            }

            questionOpen = false;
        }
    }
}