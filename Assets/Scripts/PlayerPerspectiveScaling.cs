
using UnityEngine;

public class PlayerPerspectiveScaling : MonoBehaviour
{
    [System.Serializable]
    public class RoomPerspective
    {
        public string roomName;
        public float bottomY;
        public float topY;

        [Header("Room Scale Settings")]
        [Range(0.1f, 2f)]
        public float minimumScale = 0.65f;

        [Range(0.1f, 2f)]
        public float maximumScale = 1f;
    }

    [Header("Room Perspective Settings")]
    [SerializeField] private RoomPerspective[] rooms;

    [Header("Active Room")]
    [SerializeField] private int activeRoomIndex = 0;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void LateUpdate()
    {
        if (rooms == null || rooms.Length == 0)
            return;

        if (activeRoomIndex < 0 ||
            activeRoomIndex >= rooms.Length)
            return;

        RoomPerspective currentRoom = rooms[activeRoomIndex];

        float progress = Mathf.InverseLerp(
            currentRoom.bottomY,
            currentRoom.topY,
            transform.position.y
        );

        float scaleMultiplier = Mathf.Lerp(
            currentRoom.maximumScale,
            currentRoom.minimumScale,
            progress
        );

        transform.localScale =
            originalScale * scaleMultiplier;
    }

    public void SetActiveRoom(int roomIndex)
    {
        if (rooms == null ||
            roomIndex < 0 ||
            roomIndex >= rooms.Length)
        {
            Debug.LogWarning(
                "Invalid perspective room index."
            );
            return;
        }

        activeRoomIndex = roomIndex;
    }
}
