using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DoorChoiceTransition : MonoBehaviour
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

    void Start()
    {
        doorChoicePanel.SetActive(false);
        fadePanel.alpha = 0;
    }

    void Update()
    {
        if (playerNearDoor && !choiceOpen && Input.GetKeyDown(KeyCode.E))
        {
            OpenChoiceBox();
        }
    }

    void OpenChoiceBox()
    {
        choiceOpen = true;
        doorChoicePanel.SetActive(true);
        messageText.text = message;

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(YesEnterRoom);
        noButton.onClick.AddListener(NoStayHere);
    }

    void YesEnterRoom()
    {
        StartCoroutine(TransitionRoom());
    }

    void NoStayHere()
    {
        doorChoicePanel.SetActive(false);
        choiceOpen = false;
    }

    IEnumerator TransitionRoom()
    {
        doorChoicePanel.SetActive(false);

        yield return StartCoroutine(Fade(1));

        player.position = targetSpawnPoint.position;

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

    IEnumerator Fade(float targetAlpha)
    {
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
                doorChoicePanel.SetActive(false);
                choiceOpen = false;
            }
        }
    }
}