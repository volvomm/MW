using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerTargetPosition;
    [SerializeField] private Transform cameraTargetPosition;
    [SerializeField] private Transform mainCamera;


    [Header("Lock State")]
    [SerializeField] private bool canTransition = true;

    //Its for the locked door
    // This can be use for other locked door with different required Key Item
    [Header("RequiredKey")]
    [SerializeField] private InventoryItemData requiredKey;

    public InventoryItemData RequiredKey => requiredKey;
    public bool CanTransition => canTransition;

    private void Awake()
    {
        // The door thet need 'requiredKey', its automatically locked from the first time.

        //Check the player's inventory for a specific key
        //IF they have the key, allow them to open the door "can transition =true" 
        if (requiredKey != null)
        {
            canTransition = false;
        }
    }

    // if the door is opended
    public void SetTransitionEnabled(bool enabledState)
    {
        canTransition = enabledState;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        // if the door is still locked, it does nothing.
        if (!canTransition)
            return;

        if (player != null && playerTargetPosition != null)
        {
            player.position = playerTargetPosition.position;
        }

        if (mainCamera != null && cameraTargetPosition != null)
        {

                Vector3 newCameraPosition = mainCamera.position;
                newCameraPosition.x = cameraTargetPosition.position.x;
                newCameraPosition.y = cameraTargetPosition.position.y;
                newCameraPosition.z = mainCamera.position.z;

                mainCamera.position = newCameraPosition;
        }

        Debug.Log(canTransition + " " + gameObject.name); 
    }


}