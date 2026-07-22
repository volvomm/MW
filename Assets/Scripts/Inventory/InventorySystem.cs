using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{

    public static InventorySystem Instance { get; private set; }

    [SerializeField] private int maxSlotCount = 6;
    public List<InventoryItemData> items = new List<InventoryItemData>();

    private int selectedIndex = -1;

    public IReadOnlyList<InventoryItemData> Items => items;
    public int MaxSlotCount => maxSlotCount;
    public int SelectedIndex => selectedIndex;

    public InventoryItemData SelectedItem
    {
        get
        {
            if (selectedIndex < 0 || selectedIndex >= items.Count)
                return null;

            return items[selectedIndex];
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool AddItem(InventoryItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("Adding item is null.");
            return false;
        }
        if (items.Count >= maxSlotCount)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        items.Add(item);
        Debug.Log($"got {item.itemName}");
        return true;
    }

    public bool HasItem(InventoryItemData item)
    {
        if (item == null) return false;
        return items.Contains(item);
    }

    public InventoryItemData GetItemAt(int index)
    {
        if (index < 0 || index >= items.Count)
            return null;

        return items[index];
    }

    public void SelectItem(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            selectedIndex = -1;
            return;
        }
        selectedIndex = index;
        Debug.Log($"Selected Item: {items[index].itemName}");
    }

    public void ClearSelection()
    {
        selectedIndex = -1;
    }

    public bool RemoveItem(InventoryItemData item)
    {
        if (item == null)
            return false;

        bool removed = items.Remove(item);

        if (!removed)
            return false;

        if (selectedIndex >= items.Count)
            selectedIndex = items.Count - 1;

        if (items.Count == 0)
            selectedIndex = -1;

        Debug.Log($"{item.itemName} deleted");
        return true;
    }

}
