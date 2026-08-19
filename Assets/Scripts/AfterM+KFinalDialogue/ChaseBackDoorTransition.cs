using System.Collections;
using UnityEngine;

public class ChaseBackDoorTransition : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Destination")]
    public Transform outsideSpawnPoint;

    [Header("Camera")]
    public Camera mainCamera;
    public Transform outsideCameraPoint;

    [Header("Fade")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 0.5f;

    private bool transitioning = false;

    private void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (transitioning)
            return;

        // Door should only function after the chase intro.
        if (ChaseSequenceManager.Instance == null)
            return;

        if (!ChaseSequenceManager.Instance.chaseIntroFinished)
            return;

        StartCoroutine(TransitionOutside());
    }

    private IEnumerator TransitionOutside()
    {
        transitioning = true;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.enabled = false;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Fade to black.
        yield return StartCoroutine(Fade(0f, 1f));

        // Move Patch.
        player.position = outsideSpawnPoint.position;

        // Move camera.
        Vector3 cameraPosition = outsideCameraPoint.position;

        cameraPosition.z = mainCamera.transform.position.z;

        mainCamera.transform.position = cameraPosition;

        if (ChaseSequenceManager.Instance != null)
        {
            ChaseSequenceManager.Instance.MarkReachedOutside();
        }

        // Tiny pause while black.
        yield return new WaitForSeconds(0.15f);

        // Fade back in.
        yield return StartCoroutine(Fade(1f, 0f));

        if (movement != null)
        {
            movement.enabled = true;
        }

        transitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        fadePanel.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            fadePanel.alpha =
                Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);

            yield return null;
        }

        fadePanel.alpha = endAlpha;

        if (endAlpha == 0f)
        {
            fadePanel.blocksRaycasts = false;
        }
    }
}