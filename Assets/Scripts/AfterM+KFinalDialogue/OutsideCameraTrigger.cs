using UnityEngine;

public class OutsideCameraTrigger : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCamera;

    [Header("Camera Destination")]
    public Transform cameraPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (mainCamera == null || cameraPoint == null)
            return;

        Vector3 newPosition = cameraPoint.position;

        newPosition.z = mainCamera.transform.position.z;

        mainCamera.transform.position = newPosition;
    }
}