using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public Transform holeTarget;
    public float speed = 3f;

    private bool shouldMove = false;

    public void StartMoving()
    {
        shouldMove = true;
    }

    void Update()
    {
        if (shouldMove)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                holeTarget.position,
                speed * Time.deltaTime
            );

            // If close enough → disappear
            if (Vector2.Distance(transform.position, holeTarget.position) < 0.1f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}