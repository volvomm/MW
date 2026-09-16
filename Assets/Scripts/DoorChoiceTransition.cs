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

    private bool playerNearDoor = false;
    private bool choiceOpen = false;

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

    // =========================================================
    // IINTERACTABLE
    // =========================================================

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        OpenChoiceBox();
    }

    public bool CanInteract()
    {
        return !choiceOpen;
    }

    // =========================================================
    // DOOR CHOICE
    // =========================================================

    private void OpenChoiceBox()
    {
        choiceOpen = true;

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
        StartCoroutine(TransitionRoom());
    }

    private void NoStayHere()
    {
        if (doorChoicePanel != null)
        {
            doorChoicePanel.SetActive(false);
        }

        choiceOpen = false;
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

        yield return StartCoroutine(Fade(1));

        if (player != null && targetSpawnPoint != null)
        {
            player.position = targetSpawnPoint.position;
        }

        if (mainCamera != null && targetCameraPoint != null)
        {
            mainCamera.transform.position = new Vector3(
                targetCameraPoint.position.x,
                targetCameraPoint.position.y,
                mainCamera.transform.position.z
            );
        }

        yield return StartCoroutine(Fade(0));

        choiceOpen = false;
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
        if (other.CompareTag("Player"))
        {
            playerNearDoor = false;

            if (choiceOpen)
            {
                if (doorChoicePanel != null)
                {
                    doorChoicePanel.SetActive(false);
                }

                choiceOpen = false;
            }
        }
    }
}