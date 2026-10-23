using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables;
using UnityEngine.Video;

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

    [Header("Cliff Fall Timeline")]
    public PlayableDirector cliffFallTimeline;

    [Range(0.5f, 5f)]
    public float cliffTimelineSpeed = 1.5f;

    [Header("Outside 3 Positions")]
    public Transform patchSpawn;
    public Transform patchCliffTarget;

    [Header("Camera")]
    public Camera mainCamera;
    public Transform outside3CameraPoint;

    [Header("Fade")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 0.75f;

    [Header("Ending Video Transition")]
    public VideoPlayer endingVideoPlayer;
    public GameObject endingVideoDisplay;
    public CanvasGroup endingFadeOverlay;

    public float endingDelay = 1f;
    public float endingFadeDuration = 2f;

    [Header("Patch Automatic Walk")]
    public float patchWalkSpeed = 3f;

    [Header("Devil Dog")]
    public GameObject devilDog;
    public Transform devilDogStopPoint;

    [Header("Devil Dog Entrance")]
    public Transform devilDogEntranceStartPoint;
    public Transform devilDogEntranceStopPoint;
    public float devilDogEntranceSpeed = 1.5f;

    public Animator devilDogAnimator;
    public SpriteRenderer devilDogSprite;
    public float devilDogWalkSpeed = 2.5f;

    [Tooltip("Type the EXACT name of the Devil Dog walking animation state.")]
    public string devilDogWalkAnimationState = "DevilDogWalk";

    [Header("Neutral Dog Animations")]
public string neutralDogWalkAnimationState = "NeutralDogWalk";
public string neutralDogIdleAnimationState = "NeutralDogIdle";

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public Sprite patchPortrait;
    public Sprite devilDogPortrait;

    [Header("Dialogue Settings")]
    public float typingSpeed = 0.04f;

    // Choose the dog's walking animation based on Patch's mood.
    private string GetDogWalkAnimation()
    {
        MoodManager moodManager = FindFirstObjectByType<MoodManager>();

        if (moodManager != null &&
            moodManager.currentMood != PatchMood.Frightened)
        {
            return neutralDogWalkAnimationState;
        }

        return devilDogWalkAnimationState;
    }

    // Choose the dog's idle animation based on Patch's mood.
    private string GetDogIdleAnimation()
    {
        MoodManager moodManager = FindFirstObjectByType<MoodManager>();

        if (moodManager != null &&
            moodManager.currentMood != PatchMood.Frightened)
        {
            return neutralDogIdleAnimationState;
        }

        return "DevilDogIdle";
    }

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

        // Keep Patch idle while the screen fades in.
        if (playerAnimator != null)
        {
            playerAnimator.speed = 1f;
            playerAnimator.SetFloat("Speed", 0f);
        }

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

        // Hide Patch's dialogue after the player presses E.
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // ---------------------------------
        // DEVIL DOG ENTERS
        // ---------------------------------

        // Wait one second before Devil Dog enters.
        yield return new WaitForSeconds(1f);

        // Position Devil Dog at his entrance starting point.
        if (devilDog != null &&
            devilDogEntranceStartPoint != null &&
            devilDogEntranceStopPoint != null)
        {
            devilDog.transform.position = devilDogEntranceStartPoint.position;
            devilDog.SetActive(true);

            // Devil Dog faces left, towards Patch.
            if (devilDogSprite != null)
                devilDogSprite.flipX = true;

            // Start Devil Dog's walking animation.
            if (devilDogAnimator != null)
            {
                devilDogAnimator.speed = 1f;

                string walkAnimation = GetDogWalkAnimation();

                if (!string.IsNullOrEmpty(walkAnimation))
                    devilDogAnimator.Play(walkAnimation, 0, 0f);
            }

            // Walk to the entrance stopping position.
            while (Vector2.Distance(
                devilDog.transform.position,
                devilDogEntranceStopPoint.position) > 0.05f)
            {
                Vector3 targetPosition = devilDogEntranceStopPoint.position;

                // Preserve Devil Dog's Z position.
                targetPosition.z = devilDog.transform.position.z;

                devilDog.transform.position = Vector3.MoveTowards(
                    devilDog.transform.position,
                    targetPosition,
                    devilDogEntranceSpeed * Time.deltaTime
                );

                yield return null;
            }

            // Stop at the entrance destination.
            Vector3 finalPosition = devilDogEntranceStopPoint.position;
            finalPosition.z = devilDog.transform.position.z;
            devilDog.transform.position = finalPosition;

            // Switch to the correct idle animation based on Patch's mood.
            if (devilDogAnimator != null)
            {
                devilDogAnimator.speed = 1f;
                devilDogAnimator.Play(GetDogIdleAnimation(), 0, 0f);
            }
        }

        // Devil Dog calls out to Patch.
        yield return StartCoroutine(
            ShowDialogueLine(
                "Devil Dog",
                "Hey!"
            )
        );

        // Patch turns around to face Devil Dog.
        if (patchSpriteRenderer != null)
            patchSpriteRenderer.flipX = true;

        // Patch reacts to Devil Dog's appearance.
        yield return StartCoroutine(
            ShowDialogueLine(
                "Patch",
                "What...? How did you get out?"
            )
        );

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
        "No... Go away!"
    )
);

        // ---------------------------------
        // DEVIL DOG APPROACHES THE CLIFF
        // WHILE PATCH SPEAKS
        // ---------------------------------

        yield return StartCoroutine(ShowGroundShakingDialogue());

        // Hide dialogue before the falling sequence.
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        dialogueActive = false;

        // ---------------------------------
        // PLAY CLIFF FALL TIMELINE
        // ---------------------------------

        if (cliffFallTimeline != null)
        {
            // Make sure Patch cannot move.
            if (playerMovement != null)
                playerMovement.enabled = false;

            if (playerRb != null)
                playerRb.linearVelocity = Vector2.zero;

            // Start the falling animation from the beginning.
            cliffFallTimeline.time = 0;
            cliffFallTimeline.Play();

            // Apply the custom playback speed to the entire Timeline.
            cliffFallTimeline.playableGraph.GetRootPlayable(0)
                .SetSpeed(cliffTimelineSpeed);

            // Devil Dog continues walking while Patch falls.
            StartCoroutine(ContinueDevilDogWalking());

            // Display Patch's scream while he falls.
            ShowFallingDialogue();

            // Wait until the Timeline finishes.
            while (cliffFallTimeline.state == PlayState.Playing)
            {
                yield return null;
            }

            // The falling animation has finished.
            // Keep Patch's "Nooooo!" dialogue visible.

            // Wait one frame so the E press cannot carry over.
            yield return null;

            // Wait for the player to press E.
            while (!Input.GetKeyDown(KeyCode.E))
            {
                yield return null;
            }

            // Hide Patch's dialogue before Devil Dog speaks.
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            // Wait until the next frame before starting Devil Dog's dialogue.
            yield return null;
        }
        else
        {
            Debug.LogError("Cliff Fall Timeline is not assigned!");
        }

        // ---------------------------------
        // DEVIL DOG'S FINAL REACTION
        // ---------------------------------

        // Devil Dog returns to the correct idle animation.
        if (devilDogAnimator != null)
        {
            devilDogAnimator.speed = 1f;
            devilDogAnimator.Play(GetDogIdleAnimation(), 0, 0f);
        }

        // Show Devil Dog's final dialogue.
        yield return StartCoroutine(
            ShowDialogueLine(
                "Devil Dog",
                "Dammit."
            )
        );

        yield return StartCoroutine(
    ShowDialogueLine(
        "Devil Dog",
        "I almost had him."
    )
);

        // Hide the dialogue after the player presses E.
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Start the final animated ending.
        yield return StartCoroutine(PlayEndingVideoSequence());

    } // Closes PlaySequence()

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

            string walkAnimation = GetDogWalkAnimation();

            if (!string.IsNullOrEmpty(walkAnimation))
            {
                devilDogAnimator.Play(walkAnimation, 0, 0f);
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

    private IEnumerator PlayEndingVideoSequence()
    {
        // Wait one second after Devil Dog's dialogue disappears.
        yield return new WaitForSeconds(endingDelay);

        // Slowly fade the gameplay screen to black.
        yield return StartCoroutine(EndingFade(0f, 1f));

        // Activate the video display while the screen is black.
        if (endingVideoDisplay != null)
            endingVideoDisplay.SetActive(true);

        // Prepare the video before playing it.
        if (endingVideoPlayer != null)
        {
            endingVideoPlayer.Prepare();

            while (!endingVideoPlayer.isPrepared)
            {
                yield return null;
            }

            endingVideoPlayer.Play();
        }

        // Keep the screen black for 2 seconds
        // while the animated ending begins playing.
        yield return new WaitForSeconds(2f);

        // Slowly reveal the video.
        yield return StartCoroutine(EndingFade(1f, 0f));
    }

    private IEnumerator EndingFade(float startAlpha, float endAlpha)
    {
        if (endingFadeOverlay == null)
            yield break;

        endingFadeOverlay.alpha = startAlpha;

        float timer = 0f;

        while (timer < endingFadeDuration)
        {
            timer += Time.deltaTime;

            endingFadeOverlay.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                timer / endingFadeDuration
            );

            yield return null;
        }

        endingFadeOverlay.alpha = endAlpha;
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
        Debug.Log("STARTING DIALOGUE: " + speaker + " - " + line);

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

    private IEnumerator ShowGroundShakingDialogue()
    {
        string line = "Wait, why does the ground feel unstable?";

        dialogueActive = true;
        lineFinished = false;
        advancePressed = false;

        // Display Patch's dialogue.
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (speakerNameText != null)
            speakerNameText.text = "Patch";

        if (portraitImage != null)
            portraitImage.sprite = patchPortrait;

        if (dialogueText != null)
            dialogueText.text = "";

        // Start typing Patch's dialogue.
        typingCoroutine = StartCoroutine(TypeLine(line));

        // Start Devil Dog's walking animation.
        if (devilDogAnimator != null)
        {
            devilDogAnimator.speed = 1f;

            string walkAnimation = GetDogWalkAnimation();

            if (!string.IsNullOrEmpty(walkAnimation))
            {
                devilDogAnimator.Play(walkAnimation, 0, 0f);
            }
        }

        // Start the fall when Devil Dog is this close to his stopping point.
        float fallTriggerDistance = 3.0f;

        bool dogReachedEdge = devilDog == null || devilDogStopPoint == null;

        if (dogReachedEdge)
            Debug.LogError("Devil Dog or Devil Dog Stop Point is not assigned!");

        while (!dogReachedEdge)
        {
            // Move Devil Dog toward the cliff edge.
            if (!dogReachedEdge)
            {
                devilDog.transform.position = Vector3.MoveTowards(
                    devilDog.transform.position,
                    devilDogStopPoint.position,
                    devilDogWalkSpeed * Time.deltaTime
                );

                if (Vector2.Distance(
                    devilDog.transform.position,
                    devilDogStopPoint.position) <= fallTriggerDistance)
                {
                    // Trigger Patch's fall when Devil Dog gets close enough.
                    dogReachedEdge = true;
                }
            }

            yield return null;
        }

        dialogueActive = false;
    }

    private IEnumerator ContinueDevilDogWalking()
    {
        if (devilDog == null || devilDogStopPoint == null)
            yield break;

        // Keep Devil Dog's walking animation playing.
        if (devilDogAnimator != null)
            devilDogAnimator.speed = 1f;

        // Continue walking towards the original stopping point.
        while (Vector2.Distance(
            devilDog.transform.position,
            devilDogStopPoint.position) > 0.05f)
        {
            Vector3 currentPosition = devilDog.transform.position;
            Vector3 targetPosition = devilDogStopPoint.position;

            // Preserve Devil Dog's current Z position.
            targetPosition.z = currentPosition.z;

            devilDog.transform.position = Vector3.MoveTowards(
                currentPosition,
                targetPosition,
                devilDogWalkSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Stop exactly at the edge, keeping the current Z position.
        Vector3 finalPosition = devilDogStopPoint.position;
        finalPosition.z = devilDog.transform.position.z;
        devilDog.transform.position = finalPosition;

        // Switch Devil Dog to the correct idle animation.
        if (devilDogAnimator != null)
        {
            devilDogAnimator.speed = 1f;
            devilDogAnimator.Play(GetDogIdleAnimation(), 0, 0f);
        }
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

    private void ShowFallingDialogue()
    {
        // Stop the previous dialogue from continuing to type.
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // Display the dialogue panel.
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Set Patch as the speaker.
        if (speakerNameText != null)
            speakerNameText.text = "Patch";

        // Display Patch's portrait.
        if (portraitImage != null)
            portraitImage.sprite = patchPortrait;

        // Display the final scream immediately.
        if (dialogueText != null)
            dialogueText.text = "Nooooo!";

        // No E press is required.
        dialogueActive = false;
    }
}