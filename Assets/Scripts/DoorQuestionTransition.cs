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

    [Header("Locked Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private Sprite patchPortrait;
    [SerializeField] private GameObject closeButton;

    [Header("Locked Dialogue Settings")]
    [SerializeField] private string lockedSpeakerName = "Patch";

    [TextArea(3, 6)]
    [SerializeField]
    private string lockedDialogue =
        "I think I see something in the other room.";

    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Player Movement")]
    [SerializeField] private Behaviour playerMovementScript;

    private bool playerInside;
    private bool questionOpen;

    private bool lockedDialogueActive;
    private bool isTyping;

    private Coroutine typingCoroutine;

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
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        // If the locked dialogue is currently open,
        // E either completes the sentence or closes the dialogue.
        if (lockedDialogueActive)
        {
            HandleLockedDialogueInput();
            return;
        }

        if (!playerInside || questionOpen)
        {
            return;
        }

        // Before Patch talks to the Devil Dog,
        // do not allow the closet question to open.
        if (!StoryProgress.HasTalkedToDevilDog)
        {
            StartLockedDialogue();
            return;
        }

        // After Patch talks to the Devil Dog,
        // open the normal closet question.
        OpenQuestion();
    }

    private void StartLockedDialogue()
    {
        lockedDialogueActive = true;

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
            speakerPortraitImage.enabled = patchPortrait != null;
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

        typingCoroutine = StartCoroutine(TypeLockedDialogue());
    }

    private IEnumerator TypeLockedDialogue()
    {
        isTyping = true;

        if (dialogueText == null)
        {
            isTyping = false;
            typingCoroutine = null;
            yield break;
        }

        dialogueText.text = lockedDialogue;
        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i <= lockedDialogue.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void HandleLockedDialogueInput()
    {
        if (isTyping)
        {
            CompleteLockedDialogue();
            return;
        }

        CloseLockedDialogue();
    }

    private void CompleteLockedDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = lockedDialogue;
            dialogueText.maxVisibleCharacters = lockedDialogue.Length;
        }

        isTyping = false;
    }

    private void CloseLockedDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        lockedDialogueActive = false;
        isTyping = false;

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
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = canMove;
        }

        if (!canMove &&
            player != null &&
            player.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero;
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

        while (!Mathf.Approximately(fadePanel.alpha, targetAlpha))
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

        if (lockedDialogueActive)
        {
            CloseLockedDialogue();
        }
    }
}