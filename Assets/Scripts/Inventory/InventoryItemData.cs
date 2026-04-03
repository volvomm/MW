using UnityEngine;

[CreateAssetMenu(fileName = "NewInventoryItem", menuName = "Inventory/Item Data")]

public class InventoryItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
}
