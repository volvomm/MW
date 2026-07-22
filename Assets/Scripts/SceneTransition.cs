using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public Transform playerTargetPosition;
    public Transform cameraTargetPosition;
    public Transform mainCamera;

    [Header("State")]
    public bool canTransition = true;
    public InventoryItemData requiredKey;

    // if the door is opended
    public void SetTransitionEnabled(bool enabledState)
    {
        //canTransition = enabledState;
    }

    private void Start()
    {
        if (requiredKey != null)
        {
            canTransition = false;
        }
    }

    //Check the player's inventory for a specific key
    //IF they have the key, allow them to open the door "can transition =true" 


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (requiredKey != null) //If thedoor requires a key
        {
           if (InventorySystem.Instance.items.Contains(requiredKey))
           {
                canTransition = true;
                Debug.Log("We have an item!");
           }
           else
           {
                canTransition = false;
           }
        }

        Debug.Log(canTransition + " " + gameObject.name); 

        if (canTransition)
        {

            if (player != null && playerTargetPosition != null)
            {
                // Move Player
                player.position = playerTargetPosition.position;
            }

            if (mainCamera != null && cameraTargetPosition != null)
            {
                //Move Camera
                Vector3 newCameraPosition = mainCamera.position;
                newCameraPosition.x = cameraTargetPosition.position.x;
                newCameraPosition.y = cameraTargetPosition.position.y;
                newCameraPosition.z = mainCamera.position.z;

                mainCamera.position = newCameraPosition;
            }
        }

 
    }
}