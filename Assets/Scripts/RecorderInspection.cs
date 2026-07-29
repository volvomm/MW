using UnityEngine;

public class RecorderInspection : MonoBehaviour
{
    [SerializeField] private GameObject inspectionCanvas;

    public void ShowRecorder()
    {
        inspectionCanvas.SetActive(true);
    }

    public void HideRecorder()
    {
        inspectionCanvas.SetActive(false);
    }
}