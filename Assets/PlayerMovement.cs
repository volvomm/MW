using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    public SpriteRenderer rend;
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = moveInput * moveSpeed;
        Flip();

        float animationSpeed = moveInput.sqrMagnitude > 0.001f ? 1f : 0f;
        animator.SetFloat("Speed", animationSpeed);
    }

    void Flip()
    {
        if (moveInput.x > 0)
        {
            //moving right
            rend.flipX = false;
        }
        else if (moveInput.x < 0)
        {
            //moving left
            rend.flipX = true;
        }
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
}
