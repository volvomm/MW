using UnityEngine;

public class RecorderInspection : MonoBehaviour
{
    [SerializeField] private GameObject inspectionCanvas;

    public void ShowRecorder()
    {
        if (inspectionCanvas != null)
        {
            inspectionCanvas.SetActive(true);
        }
    }

    public void HideRecorder()
    {
        if (inspectionCanvas != null)
        {
            inspectionCanvas.SetActive(false);
        }
    }
}