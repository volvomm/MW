using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private bool playerInside = false;
    private bool questionOpen = false;

    private void Start()
    {
        questionPanel.SetActive(false);
        fadePanel.alpha = 0;
    }

    private void Update()
    {
        if (playerInside && !questionOpen && Input.GetKeyDown(KeyCode.E))
        {
            OpenQuestion();
        }
    }

    private void OpenQuestion()
    {
        questionOpen = true;
        questionPanel.SetActive(true);
        questionText.text = question;

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
        questionPanel.SetActive(false);
        questionOpen = false;
    }

    private IEnumerator FadeAndMove()
    {
        questionPanel.SetActive(false);

        yield return StartCoroutine(Fade(1));

        player.position = spawnPoint.position;

        if (mainCamera != null && cameraPoint != null)
        {
            mainCamera.transform.position = new Vector3(
                cameraPoint.position.x,
                cameraPoint.position.y,
                mainCamera.transform.position.z
            );
        }

        yield return StartCoroutine(Fade(0));

        questionOpen = false;
    }

    private IEnumerator Fade(float targetAlpha)
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
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (questionOpen)
            {
                questionPanel.SetActive(false);
                questionOpen = false;
            }
        }
    }
}