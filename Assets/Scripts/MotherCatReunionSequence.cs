using System.Collections;
using UnityEngine;

public class MotherCatReunionSequence : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private GameObject patch;
    [SerializeField] private GameObject motherCat;

    [Header("Reunion Positions")]
    [SerializeField] private Transform patchReunionPosition;
    [SerializeField] private Transform motherReunionPosition;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Player Movement")]
    [SerializeField] private PlayerMovement playerMovement;

    private bool reunionStarted = false;

    public void BeginReunionTransition()
    {
        if (reunionStarted)
        {
            return;
        }

        reunionStarted = true;

        StartCoroutine(ReunionTransition());
    }

    private IEnumerator ReunionTransition()
    {
        // Stop Patch immediately before the fade.
        if (playerMovement != null)
        {
            playerMovement.StopMovementImmediately();
            playerMovement.enabled = false;
        }

        // Fade screen to black.
        yield return StartCoroutine(FadeToBlack());

        // While the player cannot see anything,
        // move Patch and Mother Cat into the kitten room.
        if (patch != null && patchReunionPosition != null)
        {
            patch.transform.position = patchReunionPosition.position;
        }

        if (motherCat != null && motherReunionPosition != null)
        {
            motherCat.transform.position = motherReunionPosition.position;
            motherCat.SetActive(true);
        }

        // Make Patch face LEFT toward the kittens.
        SetPatchFacingLeft();

        // Make Mother Cat face RIGHT toward the kittens/Patch.
        SetMotherFacingRight();

        // Fade back into the kitten room.
        yield return StartCoroutine(FadeFromBlack());

        // IMPORTANT:
        // The reunion dialogue will be started here later,
        // once we connect this to your existing dialogue system.
    }

    private IEnumerator FadeToBlack()
    {
        if (fadePanel == null)
        {
            yield break;
        }

        fadePanel.gameObject.SetActive(true);
        fadePanel.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            fadePanel.alpha =
                Mathf.Clamp01(elapsed / fadeDuration);

            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadePanel == null)
        {
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            fadePanel.alpha =
                1f - Mathf.Clamp01(elapsed / fadeDuration);

            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;

        fadePanel.gameObject.SetActive(false);
    }

    private void SetPatchFacingLeft()
    {
        if (patch == null)
        {
            return;
        }

        SpriteRenderer patchRenderer =
            patch.GetComponent<SpriteRenderer>();

        if (patchRenderer != null)
        {
            patchRenderer.flipX = true;
        }
    }

    private void SetMotherFacingRight()
    {
        if (motherCat == null)
        {
            return;
        }

        SpriteRenderer motherRenderer =
            motherCat.GetComponent<SpriteRenderer>();

        if (motherRenderer != null)
        {
            motherRenderer.flipX = false;
        }
    }
}