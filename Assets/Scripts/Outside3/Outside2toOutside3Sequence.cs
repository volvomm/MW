using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Outside2ToOutside3Sequence : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public PlayerMovement playerMovement;
    public Animator playerAnimator;
    public SpriteRenderer playerSprite;

    [Header("Patch Facing")]
    public SpriteRenderer patchSpriteRenderer;

    [Header("Cliff Fall Sequence")]
    public Transform patchFallEndPoint;
    public Transform devilDogCliffStopPoint;
    public float cliffFallDuration = 1.5f;

    [Header("Outside 3 Positions")]
    public Transform patchSpawn;
    public Transform patchCliffTarget;

    [Header("Camera")]
    public Camera mainCamera;
    public Transform outside3CameraPoint;

    [Header("Fade")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 0.75f;

    [Header("Patch Automatic Walk")]
    public float patchWalkSpeed = 3f;

    [Header("Devil Dog")]
    public GameObject devilDog;
    public Transform devilDogStopPoint;
    public Animator devilDogAnimator;
    public SpriteRenderer devilDogSprite;
    public float devilDogWalkSpeed = 2.5f;

    [Tooltip("Type the EXACT name of the Devil Dog walking animation state.")]
    public string devilDogWalkAnimationState = "DevilDogWalk";

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public Sprite patchPortrait;
    public Sprite devilDogPortrait;

    [Header("Dialogue Settings")]
    public float typingSpeed = 0.04f;

    private Rigidbody2D playerRb;

    private bool sequenceStarted = false;
    private bool dialogueActive = false;
    private bool lineFinished = false;
    private bool advancePressed = false;
    private bool patchIsFalling = false;

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (player != null)
            playerRb = player.GetComponent<Rigidbody2D>();

        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void LateUpdate()
    {
        if (patchIsFalling && playerSprite != null)
        {
            playerSprite.flipX = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (sequenceStarted)
            return;

        if (!other.CompareTag("Player"))
            return;

        sequenceStarted = true;

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // ---------------------------------
        // DISABLE PLAYER CONTROL
        // ---------------------------------

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        // ---------------------------------
        // FADE TO BLACK
        // ---------------------------------

        yield return StartCoroutine(Fade(0f, 1f));

        // Move Patch to Outside 3.
        if (player != null && patchSpawn != null)
            player.position = patchSpawn.position;

        // Move camera to Outside 3.
        if (mainCamera != null && outside3CameraPoint != null)
        {
            Vector3 cameraPosition = outside3CameraPoint.position;
            cameraPosition.z = mainCamera.transform.position.z;

            mainCamera.transform.position = cameraPosition;
        }

        // Make Patch face right.
        if (playerSprite != null)
            playerSprite.flipX = false;

        // ---------------------------------
        // FADE BACK IN
        // ---------------------------------

        yield return StartCoroutine(Fade(1f, 0f));

        // ---------------------------------
        // PATCH WALKS TO CLIFF
        // ---------------------------------

        if (playerSprite != null)
            playerSprite.flipX = false;

        if (playerAnimator != null)
            playerAnimator.SetFloat("Speed", patchWalkSpeed);

        if (player != null && patchCliffTarget != null)
        {
            while (Vector2.Distance(
                       player.position,
                       patchCliffTarget.position) > 0.05f)
            {
                player.position = Vector3.MoveTowards(
                    player.position,
                    patchCliffTarget.position,
                    patchWalkSpeed * Time.deltaTime
                );

                yield return null;
            }

            player.position = patchCliffTarget.position;
        }

        // Return Patch to idle.
        if (playerAnimator != null)
            playerAnimator.SetFloat("Speed", 0f);

        // ---------------------------------
        // PATCH CLIFF DIALOGUE
        // ---------------------------------

        yield return StartCoroutine(
            ShowDialogueLine(
                "Patch",
                "Oh man, this is really high..."
            )
        );

        // ---------------------------------
        // DEVIL DOG ENTERS
        // ---------------------------------

        if (devilDog != null)
            devilDog.SetActive(true);

        // Patch turns around to look at Devil Dog.
        if (patchSpriteRenderer != null)
            patchSpriteRenderer.flipX = true;

        // Devil Dog faces toward Patch.
        if (devilDogSprite != null)
            devilDogSprite.flipX = true;

        // Start Devil Dog walking animation.
        if (devilDogAnimator != null &&
            !string.IsNullOrEmpty(devilDogWalkAnimationState))
        {
            devilDogAnimator.speed = 1f;
            devilDogAnimator.Play(devilDogWalkAnimationState);
        }

        // Move Devil Dog to his first stopping point.
        if (devilDog != null && devilDogStopPoint != null)
        {
            while (Vector2.Distance(
                       devilDog.transform.position,
                       devilDogStopPoint.position) > 0.05f)
            {
                devilDog.transform.position = Vector3.MoveTowards(
                    devilDog.transform.position,
                    devilDogStopPoint.position,
                    devilDogWalkSpeed * Time.deltaTime
                );

                yield return null;
            }

            devilDog.transform.position = devilDogStopPoint.position;
        }

        // Freeze Devil Dog after reaching Patch.
        if (devilDogAnimator != null)
            devilDogAnimator.speed = 0f;

        // ---------------------------------
        // DEVIL DOG DIALOGUE
        // ---------------------------------

        yield return StartCoroutine(
            ShowDialogueLine(
                "Devil Dog",
                "How dare you trick me and lock me in the closet!"
            )
        );

        yield return StartCoroutine(
            ShowDialogueLine(
                "Devil Dog",
                "You're going to regret doing that."
            )
        );

        // ---------------------------------
        // PATCH RESPONSE
        // ---------------------------------

        yield return StartCoroutine(
            ShowDialogueLine(
                "Patch",
                "Please no..."
            )
        );

        // Hide dialogue before the falling sequence.
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        dialogueActive = false;

        // ---------------------------------
        // PATCH FALLS + DEVIL DOG WALKS
        // ---------------------------------

        yield return StartCoroutine(CliffFallSequence());

        // Patch intentionally remains disabled
        // after falling off the cliff.
    }

    private IEnumerator CliffFallSequence()
    {
        // ---------------------------------
        // SAFETY CHECKS
        // ---------------------------------

        if (player == null)
        {
            Debug.LogError("Player has not been assigned.");
            yield break;
        }

        if (patchFallEndPoint == null)
        {
            Debug.LogError("Patch Fall End Point has not been assigned.");
            yield break;
        }

        if (devilDog == null)
        {
            Debug.LogError("Devil Dog has not been assigned.");
            yield break;
        }

        if (devilDogCliffStopPoint == null)
        {
            Debug.LogError("Devil Dog Cliff Stop Point has not been assigned.");
            yield break;
        }

        // ---------------------------------
        // STOP PATCH MOVEMENT
        // ---------------------------------

        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        // ---------------------------------
        // START PATCH FALL ANIMATION
        // ---------------------------------

        if (playerAnimator != null)
        {
            playerAnimator.speed = 1f;
            playerAnimator.SetFloat("Speed", 0f);

            // Immediately switch Patch from his normal sprite
            // to the first frame of the falling animation.
            playerAnimator.Play("PatchFall", 0, 0f);

            // Force the Animator to update immediately,
            // so the falling sprite is loaded THIS frame.
            playerAnimator.Update(0f);
        }

        // NOW change Flip X.
        // At this point Patch is already using a falling sprite,
        // so there is no normal idle sprite being flipped right.
        if (playerSprite != null)
            playerSprite.flipX = false;

        patchIsFalling = true;

        // ---------------------------------
        // START DEVIL DOG WALKING AGAIN
        // ---------------------------------

        if (devilDogAnimator != null)
        {
            devilDogAnimator.speed = 1f;

            if (!string.IsNullOrEmpty(devilDogWalkAnimationState))
            {
                devilDogAnimator.Play(devilDogWalkAnimationState);
            }
        }

        // ---------------------------------
        // STORE START / END POSITIONS
        // ---------------------------------

        Vector3 patchStartPosition = player.position;
        Vector3 patchEndPosition = patchFallEndPoint.position;

        Vector3 dogStartPosition = devilDog.transform.position;
        Vector3 dogEndPosition = devilDogCliffStopPoint.position;

        float elapsedTime = 0f;

        // ---------------------------------
        // MOVE BOTH AT THE SAME TIME
        // ---------------------------------

        while (elapsedTime < cliffFallDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsedTime / cliffFallDuration
            );

            // Patch physically moves down past the screen.
            player.position = Vector3.Lerp(
                patchStartPosition,
                patchEndPosition,
                t
            );

            // Devil Dog moves toward the middle of the cliff.
            devilDog.transform.position = Vector3.Lerp(
                dogStartPosition,
                dogEndPosition,
                t
            );

            yield return null;
        }

        // ---------------------------------
        // FINISH BOTH MOVEMENTS EXACTLY
        // ---------------------------------

        player.position = patchEndPosition;
        devilDog.transform.position = dogEndPosition;

        // ---------------------------------
        // STOP DEVIL DOG
        // ---------------------------------

        if (devilDogAnimator != null)
            devilDogAnimator.speed = 0f;

        // ---------------------------------
        // HIDE PATCH ONLY AFTER HE HAS
        // ALREADY MOVED BELOW THE CAMERA
        // ---------------------------------

        patchIsFalling = false;
        player.gameObject.SetActive(false);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (fadePanel == null)
            yield break;

        fadePanel.blocksRaycasts = true;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            fadePanel.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                timer / fadeDuration
            );

            yield return null;
        }

        fadePanel.alpha = endAlpha;

        if (endAlpha == 0f)
            fadePanel.blocksRaycasts = false;
    }

    private IEnumerator ShowDialogueLine(string speaker, string line)
    {
        dialogueActive = true;
        lineFinished = false;
        advancePressed = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (speakerNameText != null)
            speakerNameText.text = speaker;

        if (portraitImage != null)
        {
            if (speaker == "Patch")
                portraitImage.sprite = patchPortrait;
            else if (speaker == "Devil Dog")
                portraitImage.sprite = devilDogPortrait;
        }

        if (dialogueText != null)
            dialogueText.text = "";

        typingCoroutine = StartCoroutine(TypeLine(line));

        while (!advancePressed)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!lineFinished)
                {
                    if (typingCoroutine != null)
                        StopCoroutine(typingCoroutine);

                    if (dialogueText != null)
                        dialogueText.text = line;

                    lineFinished = true;
                }
                else
                {
                    advancePressed = true;
                }
            }

            yield return null;
        }

        dialogueActive = false;
    }

    private IEnumerator TypeLine(string line)
    {
        if (dialogueText == null)
        {
            lineFinished = true;
            yield break;
        }

        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        lineFinished = true;
    }
}