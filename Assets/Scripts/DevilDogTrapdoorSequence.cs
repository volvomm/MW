using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevilDogTrapdoorSequence : MonoBehaviour
{
    [Header("Devil Dog")]
    [SerializeField] private GameObject devilDog;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private GameObject closeButton;

    [Header("Portraits")]
    [SerializeField] private Sprite devilDogPortrait;

    [Header("Recorder Recording Dialogue")]
    [SerializeField] private BoxDialogue recorderRecordingDialogue;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Animator playerAnimator;

    [Header("Devil Dog Dialogue")]
    [TextArea(2, 5)]
    [SerializeField]
    private string firstDogLine = "The kittens are in here?";

    [TextArea(2, 5)]
    [SerializeField]
    private string finalDogLine = "Those kittens are in there!";

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.04f;

    private bool sequenceActive;
    private bool isTyping;
    private bool waitingForInput;

    private int sequenceStage;

    private Coroutine typingCoroutine;
    private string currentFullText;

    private void Start()
    {
        if (devilDog != null)
        {
            devilDog.SetActive(false);
        }
    }

    private void Update()
    {
        if (!sequenceActive)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        // If text is currently typing, E only finishes
        // the current sentence immediately.
        if (isTyping)
        {
            CompleteCurrentTextImmediately();
            return;
        }

        // Do not advance unless the current line
        // has completely finished.
        if (!waitingForInput)
        {
            return;
        }

        waitingForInput = false;

        AdvanceSequence();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (sequenceActive)
        {
            return;
        }

        if (StoryProgress.DevilDogTrapdoorSequenceFinished)
        {
            return;
        }

        // This scene only happens after the second
        // Devil Dog lure conversation has finished.
        if (!StoryProgress.DevilDogLureDialogueFinished)
        {
            return;
        }

        StartSequence();
    }

    private void StartSequence()
    {
        sequenceActive = true;
        sequenceStage = 0;

        FreezePlayer();

        if (devilDog != null)
        {
            devilDog.SetActive(true);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (closeButton != null)
        {
            closeButton.SetActive(false);
        }

        ShowDevilDogLine(firstDogLine);
    }

    private void AdvanceSequence()
    {
        sequenceStage++;

        // STAGE 1:
        // Play ONLY the actual recorded meow line.
        if (sequenceStage == 1)
        {
            ShowRecordedMeow();
            return;
        }

        // STAGE 2:
        // Devil Dog reacts to hearing the recording.
        if (sequenceStage == 2)
        {
            ShowDevilDogLine(finalDogLine);
            return;
        }

        // STAGE 3:
        // Devil Dog disappears into the closet.
        if (sequenceStage == 3)
        {
            EndSequence();
        }
    }

    private void ShowDevilDogLine(string sentence)
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = "Devil Dog";
        }

        if (speakerPortraitImage != null)
        {
            speakerPortraitImage.sprite = devilDogPortrait;
            speakerPortraitImage.enabled =
                devilDogPortrait != null;
        }

        StartTyping(sentence, typingSpeed);
    }

    private void ShowRecordedMeow()
    {
        if (recorderRecordingDialogue == null)
        {
            Debug.LogWarning(
                "Recorder Recording Dialogue has not been assigned.",
                this
            );

            ShowDevilDogLine(finalDogLine);
            sequenceStage = 2;
            return;
        }

        if (recorderRecordingDialogue.dialogueLines == null ||
            recorderRecordingDialogue.dialogueLines.Length < 2)
        {
            Debug.LogWarning(
                "Recorder Recording Dialogue needs at least 2 dialogue lines.",
                this
            );

            ShowDevilDogLine(finalDogLine);
            sequenceStage = 2;
            return;
        }

        if (speakerNameText != null)
        {
            speakerNameText.text =
                recorderRecordingDialogue.npcName;
        }

        if (speakerPortraitImage != null)
        {
            speakerPortraitImage.sprite =
                recorderRecordingDialogue.npcPortrait;

            speakerPortraitImage.enabled =
                recorderRecordingDialogue.npcPortrait != null;
        }

        // Element 1 is the actual:
        // "m-meow mmeow m-m-meoww"
        // recording line.
        string recordedMeow =
            recorderRecordingDialogue.dialogueLines[1];

        StartTyping(
            recordedMeow,
            recorderRecordingDialogue.typingSpeed
        );
    }

    private void StartTyping(string sentence, float speed)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentFullText = sentence;

        typingCoroutine =
            StartCoroutine(TypeCurrentLine(sentence, speed));
    }

    private IEnumerator TypeCurrentLine(
        string sentence,
        float speed
    )
    {
        isTyping = true;
        waitingForInput = false;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        foreach (char character in sentence)
        {
            if (dialogueText != null)
            {
                dialogueText.text += character;
            }

            yield return new WaitForSeconds(speed);
        }

        if (dialogueText != null)
        {
            dialogueText.text = sentence;
        }

        isTyping = false;
        waitingForInput = true;
        typingCoroutine = null;
    }

    private void CompleteCurrentTextImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = currentFullText;
        }

        isTyping = false;
        waitingForInput = true;
    }

    private void EndSequence()
    {
        sequenceActive = false;

        StoryProgress.DevilDogTrapdoorSequenceFinished = true;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (devilDog != null)
        {
            devilDog.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.SetActive(true);
        }

        UnfreezePlayer();
    }

    private void FreezePlayer()
    {
        // Stop physical movement immediately.
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        // Force the walking animation back to idle BEFORE
        // disabling PlayerMovement.
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
        }

        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
            playerMovement.enabled = false;
        }
    }

    private void UnfreezePlayer()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
        }

        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
            playerMovement.enabled = true;
        }
    }
}