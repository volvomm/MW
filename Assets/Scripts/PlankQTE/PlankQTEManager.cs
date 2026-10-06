using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlankQTEManager : MonoBehaviour
{
    [System.Serializable]
    public class QTEBox
    {
        public GameObject boxObject;
        public TMP_Text keyText;
        public Image timerFill;
        public Image keyBackground;
    }

    [Header("QTE Boxes")]
    [SerializeField] private QTEBox[] qteBoxes;

    [Header("QTE Settings")]
    [SerializeField] private float timePerKey = 2f;
    [SerializeField] private float flashDuration = 0.6f;

    // Prevents the E press used to interact
    // from also counting as a QTE input.
    [SerializeField] private float startingInputDelay = 0.12f;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform playerTransform;

    [Header("QTE Position")]
    [SerializeField] private RectTransform qteContainer;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private float horizontalOffset = 220f;
    [SerializeField] private float verticalOffset = 40f;

    private readonly KeyCode[] possibleKeys =
    {
        KeyCode.A,
        KeyCode.B,
        KeyCode.C,
        KeyCode.D,
        KeyCode.F,
        KeyCode.G,
        KeyCode.H,
        KeyCode.J,
        KeyCode.K,
        KeyCode.L,
        KeyCode.Q,
        KeyCode.R,
        KeyCode.S,
        KeyCode.T,
        KeyCode.V,
        KeyCode.W,
        KeyCode.X,
        KeyCode.Y,
        KeyCode.Z
    };

    private KeyCode[] selectedKeys;

    private int currentBoxIndex;
    private float currentTimer;
    private float inputDelayTimer;

    private bool qteRunning;
    private bool waitingDuringFlash;
    private bool qteSuccessful;

    private Color[] normalBackgroundColors;

    // Only ONE of these will be used for each QTE.
    private WoodenPlankPickup currentPlankPickup;
    private WoodenPlankBarricade currentBarricade;

    private void Awake()
    {
        selectedKeys = new KeyCode[3];
        normalBackgroundColors = new Color[3];

        for (int i = 0; i < qteBoxes.Length; i++)
        {
            if (qteBoxes[i].keyBackground != null)
            {
                normalBackgroundColors[i] =
                    qteBoxes[i].keyBackground.color;
            }
        }
    }

    private void Start()
    {
        HideQTE();
    }

    private void Update()
    {
        if (!qteRunning || waitingDuringFlash)
        {
            return;
        }

        // Ignore the original E interaction press.
        if (inputDelayTimer > 0f)
        {
            inputDelayTimer -= Time.deltaTime;
            return;
        }

        UpdateTimer();
        CheckKeyboardInput();
    }

    private void PositionQTENextToPlayer()
    {
        if (qteContainer == null ||
            playerTransform == null ||
            gameCamera == null ||
            playerMovement == null)
        {
            return;
        }

        Vector3 patchScreenPosition =
            gameCamera.WorldToScreenPoint(
                playerTransform.position
            );

        bool patchFacingLeft =
            playerMovement.rend != null &&
            playerMovement.rend.flipX;

        float xOffset =
            patchFacingLeft
                ? -horizontalOffset
                : horizontalOffset;

        qteContainer.position = new Vector3(
            patchScreenPosition.x + xOffset,
            patchScreenPosition.y + verticalOffset,
            0f
        );
    }

    // =====================================================
    // FLOOR PLANK QTE
    // =====================================================

    public void StartQTE(WoodenPlankPickup plankPickup)
    {
        if (qteRunning)
        {
            return;
        }

        currentPlankPickup = plankPickup;
        currentBarricade = null;

        StartCommonQTE();
    }

    // =====================================================
    // CLOSET BARRICADE QTE
    // =====================================================

    public void StartBarricadeQTE(
        WoodenPlankBarricade barricade)
    {
        if (qteRunning)
        {
            return;
        }

        currentBarricade = barricade;
        currentPlankPickup = null;

        StartCommonQTE();
    }

    // =====================================================
    // SHARED QTE START
    // =====================================================

    private void StartCommonQTE()
    {
        if (qteBoxes == null ||
            qteBoxes.Length != 3)
        {
            Debug.LogWarning(
                "PlankQTEManager needs exactly 3 QTE boxes."
            );

            currentPlankPickup = null;
            currentBarricade = null;

            return;
        }

        qteSuccessful = true;

        GenerateRandomKeys();

        PositionQTENextToPlayer();

        currentBoxIndex = 0;
        qteRunning = true;
        waitingDuringFlash = false;

        // Keeps the E interaction press from
        // instantly failing QTE #1.
        inputDelayTimer = startingInputDelay;

        for (int i = 0; i < qteBoxes.Length; i++)
        {
            if (qteBoxes[i].boxObject != null)
            {
                qteBoxes[i].boxObject.SetActive(true);
            }

            if (qteBoxes[i].timerFill != null)
            {
                qteBoxes[i].timerFill.fillAmount = 1f;
            }

            if (qteBoxes[i].keyBackground != null)
            {
                qteBoxes[i].keyBackground.color =
                    normalBackgroundColors[i];
            }
        }

        FreezePlayer();

        BeginCurrentBox();
    }

    private void GenerateRandomKeys()
    {
        for (int i = 0; i < 3; i++)
        {
            KeyCode newKey;
            bool duplicate;

            do
            {
                newKey =
                    possibleKeys[
                        Random.Range(
                            0,
                            possibleKeys.Length
                        )
                    ];

                duplicate = false;

                for (int j = 0; j < i; j++)
                {
                    if (selectedKeys[j] == newKey)
                    {
                        duplicate = true;
                        break;
                    }
                }
            }
            while (duplicate);

            selectedKeys[i] = newKey;

            if (qteBoxes[i].keyText != null)
            {
                qteBoxes[i].keyText.text =
                    newKey.ToString();
            }
        }
    }

    private void BeginCurrentBox()
    {
        currentTimer = timePerKey;

        if (qteBoxes[currentBoxIndex].timerFill != null)
        {
            qteBoxes[currentBoxIndex]
                .timerFill.fillAmount = 1f;
        }
    }

    private void UpdateTimer()
    {
        currentTimer -= Time.deltaTime;

        float amount =
            Mathf.Clamp01(
                currentTimer / timePerKey
            );

        if (qteBoxes[currentBoxIndex].timerFill != null)
        {
            qteBoxes[currentBoxIndex]
                .timerFill.fillAmount = amount;
        }

        if (currentTimer <= 0f)
        {
            FailCurrentBox();
        }
    }

    private void CheckKeyboardInput()
    {
        if (!Input.anyKeyDown)
        {
            return;
        }

        KeyCode correctKey =
            selectedKeys[currentBoxIndex];

        if (Input.GetKeyDown(correctKey))
        {
            CompleteCurrentBox();
            return;
        }

        if (AnyLetterKeyPressed())
        {
            FailCurrentBox();
        }
    }

    private bool AnyLetterKeyPressed()
    {
        for (KeyCode key = KeyCode.A;
             key <= KeyCode.Z;
             key++)
        {
            if (Input.GetKeyDown(key))
            {
                return true;
            }
        }

        return false;
    }

    private void CompleteCurrentBox()
    {
        if (qteBoxes[currentBoxIndex].timerFill != null)
        {
            qteBoxes[currentBoxIndex]
                .timerFill.fillAmount = 0f;
        }

        MoveToNextBox();
    }

    private void FailCurrentBox()
    {
        if (waitingDuringFlash)
        {
            return;
        }

        qteSuccessful = false;

        StartCoroutine(FlashFailedBox());
    }

    private IEnumerator FlashFailedBox()
    {
        waitingDuringFlash = true;

        Image background =
            qteBoxes[currentBoxIndex].keyBackground;

        if (background != null)
        {
            background.color = Color.red;
        }

        yield return new WaitForSeconds(
            flashDuration
        );

        if (background != null)
        {
            background.color =
                normalBackgroundColors[
                    currentBoxIndex
                ];
        }

        waitingDuringFlash = false;

        MoveToNextBox();
    }

    private void MoveToNextBox()
    {
        currentBoxIndex++;

        if (currentBoxIndex >= 3)
        {
            FinishQTE();
            return;
        }

        BeginCurrentBox();
    }

    private void FinishQTE()
    {
        qteRunning = false;

        HideQTE();

        UnfreezePlayer();

        if (qteSuccessful)
        {
            Debug.Log("Plank QTE SUCCESS!");

            // Floor plank pickup.
            if (currentPlankPickup != null)
            {
                currentPlankPickup.QTESucceeded();
            }

            // Closet plank placement.
            if (currentBarricade != null)
            {
                currentBarricade.QTESucceeded();
            }
        }
        else
        {
            Debug.Log("Plank QTE FAILED!");

            // Floor plank pickup failed.
            if (currentPlankPickup != null)
            {
                currentPlankPickup.QTEFailed();
            }

            // Closet placement failed.
            if (currentBarricade != null)
            {
                currentBarricade.QTEFailed();
            }
        }

        currentPlankPickup = null;
        currentBarricade = null;
    }

    private void FreezePlayer()
    {
        if (playerMovement == null)
        {
            return;
        }

        playerMovement.StopMovementImmediately();
        playerMovement.enabled = false;
    }

    private void UnfreezePlayer()
    {
        if (playerMovement == null)
        {
            return;
        }

        playerMovement.StopMovementImmediately();
        playerMovement.enabled = true;
    }

    private void HideQTE()
    {
        if (qteBoxes == null)
        {
            return;
        }

        foreach (QTEBox box in qteBoxes)
        {
            if (box.boxObject != null)
            {
                box.boxObject.SetActive(false);
            }
        }
    }
}