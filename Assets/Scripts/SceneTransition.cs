using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    public Transform player;
    public Transform playerTargetPosition;
    public Transform cameraTargetPosition;
    public Transform mainCamera;

    private void OnTriggerEnter2D(Collider2D collision)
    {
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