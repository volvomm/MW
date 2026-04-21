using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public Transform playerTargetPosition;
    public Transform cameraTargetPosition;
    public Transform mainCamera;

    [SerializeField] private bool canTransition = false;

    // if the door is opended
    public void SetTransitionEnabled(bool enabledState)
    {
        canTransition = enabledState;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (!canTransition)
            return;

        if (collision.CompareTag("Player"))
        {
            // Move Player
            player.position = playerTargetPosition.position;

            //Move Camera
            Vector3 newCameraPosition = mainCamera.position;
            newCameraPosition.x = cameraTargetPosition.position.x;
            newCameraPosition.y = cameraTargetPosition.position.y;
            newCameraPosition.z = mainCamera.position.z;

            mainCamera.position = newCameraPosition;
        }
    }
}