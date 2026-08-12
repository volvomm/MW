using UnityEngine;

public class DevilDogWalkOff : MonoBehaviour
{
    [Header("Walk Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private Transform exitPoint;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private bool isWalkingAway = false;

    private void Update()
    {
        if (!isWalkingAway)
        {
            return;
        }

        // Move the Devil Dog to the left.
        transform.position += Vector3.left * walkSpeed * Time.deltaTime;

        // Once the dog reaches the exit point, remove him from this room.
        if (exitPoint != null && transform.position.x <= exitPoint.position.x)
        {
            isWalkingAway = false;
            gameObject.SetActive(false);
        }
    }

    public void StartWalkingAway()
    {
        if (isWalkingAway)
        {
            return;
        }

        isWalkingAway = true;

        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("DevilDogWalk", 0, 0f);
        }
    }
}