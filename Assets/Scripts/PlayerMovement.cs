
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [Header("Shadow Settings")]
    [SerializeField] private Transform shadow;
    [SerializeField] private float shadowOffsetX = 3.4f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public SpriteRenderer rend;

    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        UpdateShadowPosition();
    }

    private void Update()
    {
        Flip();

        float animationSpeed =
            moveInput.sqrMagnitude > 0.001f ? 1f : 0f;

        animator.SetFloat("Speed", animationSpeed);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void Flip()
    {
        if (moveInput.x > 0)
        {
            // Moving right.
            rend.flipX = false;
        }
        else if (moveInput.x < 0)
        {
            // Moving left.
            rend.flipX = true;
        }

        UpdateShadowPosition();
    }

    private void UpdateShadowPosition()
    {
        if (shadow == null || rend == null)
            return;

        Vector3 shadowPosition = shadow.localPosition;

        shadowPosition.x = rend.flipX
            ? -shadowOffsetX
            : shadowOffsetX;

        shadow.localPosition = shadowPosition;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }

    public void StopMovementImmediately()
    {
        moveInput = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }
}
