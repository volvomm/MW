using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DoorChoiceTransition : MonoBehaviour, IInteractable
{
    [Header("UI")]
    public GameObject doorChoicePanel;
    public TextMeshProUGUI messageText;
    public Button yesButton;
    public Button noButton;
    public CanvasGroup fadePanel;

    [Header("Player")]
    public Transform player;

    [Header("Transition")]
    public Transform targetSpawnPoint;
    public Camera mainCamera;
    public Transform targetCameraPoint;

    [Header("Settings")]
    public float fadeSpeed = 1f;
    public string message = "Would you like to enter the next room?";

    // =========================================================
    // OPTIONAL KITTEN STORY GATE
    // =========================================================

    [Header("Optional Kitten Story Gate")]
    [Tooltip(
        "Turn this on ONLY for the kittens-room door that leads " +
        "to the trapdoor room."
    )]
    public bool requireKittenGroupDialogue = false;

    [TextArea(2, 4)]
    public string kittenLockedDialogue =
        "I should talk to those kittens first.";

    [Header("Locked Door Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI dialogueNameText;
    public Image dialoguePortraitImage;
    public Sprite patchPortrait;

    [Header("Player Movement Lock")]
    public PlayerMovement playerMovement;
    public Rigidbody2D playerRigidbody;
    public Animator playerAnimator;

    private bool playerNearDoor = false;
    private bool choiceOpen = false;

    private bool lockedDialogueActive = false;
    private bool waitingForERelease = false;

    private void Start()
    {
        if (doorChoicePanel != null)
        {
            doorChoicePanel.SetActive(false);
        }

        if (fadePanel != null)
        {
            fadePanel.alpha = 0;
        }
    }

    private void Update()
    {
        // This Update only handles the special
        // "talk to the kittens first" dialogue.
        if (!lockedDialogueActive)
        {
            return;
        }

        // Prevent the same E press that opened the dialogue
        // from immediately closing it.
        if (waitingForERelease)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                waitingForERelease = false;
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            CloseLockedDialogue();
        }
    }

    // =========================================================
    // IINTERACTABLE
    // =========================================================

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        // =====================================================
        // KITTEN STORY GATE
        // =====================================================

        if (requireKittenGroupDialogue &&
            !StoryProgress.KittenGroupDialogueFinished)
        {
            OpenLockedDialogue();
            return;
        }

        // Kitten dialogue has been completed,
        // or this door doesn't use the kitten gate.
        OpenChoiceBox();
    }

    public bool CanInteract()
    {
        return !choiceOpen && !lockedDialogueActive;
    }

    // =========================================================
    // LOCKED KITTEN DOOR DIALOGUE
    // =========================================================

    private void OpenLockedDialogue()
    {
        lockedDialogueActive = true;
        waitingForERelease = true;

        FreezePlayer();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (dialogueNameText != null)
        {
            dialogueNameText.text = "Patch";
        }

        if (dialogueText != null)
        {
            dialogueText.text = kittenLockedDialogue;
        }

        if (dialoguePortraitImage != null)
        {
            dialoguePortraitImage.sprite = patchPortrait;
            dialoguePortraitImage.enabled =
                patchPortrait != null;
        }
    }

    private void CloseLockedDialogue()
    {
        lockedDialogueActive = false;
        waitingForERelease = false;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        UnfreezePlayer();
    }

    // =========================================================
    // DOOR CHOICE
    // =========================================================

    private void OpenChoiceBox()
    {
        choiceOpen = true;

        // NEW:
        // Patch cannot move while the Yes/No
        // question is on screen.
        FreezePlayer();

        if (doorChoicePanel != null)
        {
            doorChoicePanel.SetActive(true);
        }

        if (messageText != null)
        {
            messageText.text = message;
        }

        if (yesButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(YesEnterRoom);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(NoStayHere);
        }
    }

    private void YesEnterRoom()
    {
        // Patch remains frozen.
        StartCoroutine(TransitionRoom());
    }

    private void NoStayHere()
    {
        if (doorChoicePanel != null)
        {
            doorChoicePanel.SetActive(false);
        }

        choiceOpen = false;

        // NEW:
        // Choosing No closes the prompt,
        // so Patch can move again.
        UnfreezePlayer();
    }

    // =========================================================
    // ROOM TRANSITION
    // =========================================================

    private IEnumerator TransitionRoom()
    {
        if (doorChoicePanel != null)
        {
            doorChoicePanel.SetActive(false);
        }

        // Keep Patch frozen during the entire transition.
        FreezePlayer();

        // Fade to black.
        yield return StartCoroutine(Fade(1));

        // Move Patch to the new room.
        if (player != null &&
            targetSpawnPoint != null)
        {
            player.position =
                targetSpawnPoint.position;
        }

        // Move the camera to the new room.
        if (mainCamera != null &&
            targetCameraPoint != null)
        {
            mainCamera.transform.position =
                new Vector3(
                    targetCameraPoint.position.x,
                    targetCameraPoint.position.y,
                    mainCamera.transform.position.z
                );
        }

        // Fade back in.
        yield return StartCoroutine(Fade(0));

        choiceOpen = false;

        // NEW:
        // Only restore movement once the entire
        // room transition has finished.
        UnfreezePlayer();
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
            fadePanel.alpha =
                Mathf.MoveTowards(
                    fadePanel.alpha,
                    targetAlpha,
                    fadeSpeed * Time.deltaTime
                );

            yield return null;
        }

        fadePanel.alpha = targetAlpha;
    }

    // =========================================================
    // PLAYER MOVEMENT LOCK
    // =========================================================

    private void FreezePlayer()
    {
        // Immediately stop any current velocity.
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;
        }

        // Stop Patch's walking animation.
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat(
                "Speed",
                0f
            );
        }

        // Disable player movement.
        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
            playerMovement.enabled = false;
        }
    }

    private void UnfreezePlayer()
    {
        // Make sure Patch doesn't retain any
        // old movement velocity.
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat(
                "Speed",
                0f
            );
        }

        // Restore player movement.
        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
            playerMovement.enabled = true;
        }
    }

    // =========================================================
    // PLAYER RANGE
    // =========================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearDoor = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerNearDoor = false;

        // Safety:
        // If the question somehow closes because Patch
        // leaves the trigger, restore his movement.
        if (choiceOpen)
        {
            if (doorChoicePanel != null)
            {
                doorChoicePanel.SetActive(false);
            }

            choiceOpen = false;

            UnfreezePlayer();
        }
    }
}