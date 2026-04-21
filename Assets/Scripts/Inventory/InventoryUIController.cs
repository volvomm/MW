using UnityEngine;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private List<InventorySlotUI> slots = new List<InventorySlotUI>();

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode altToogleKey = KeyCode.RightShift;

    [Header("State")]
    [SerializeField] private bool isOpen = false;

    private bool isSelectionMode = false;
    private IIventoryItemRecivers currentReceiver;

    public bool IsOpen => isOpen;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        SetInventoryVisiable(false);
        InitializeSlots();
        RefreshUI();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(altToogleKey))
        {
            if (isSelectionMode)
                return;

            ToggleInventory();
        }
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null)
            {
                slots[i].Initialize(this, i);
            }
        }
    }

    public void RefreshUI()
    {
        if (InventorySystem.Instance == null)
            return;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
                continue;

            InventoryItemData item = InventorySystem.Instance.GetItemAt(i);

            if (item == null)
                slots[i].SetEmpty();
            else
                slots[i].SetItem(item.itemIcon);

            bool isSelected = InventorySystem.Instance.SelectedIndex == i;
            slots[i].SetSelected(isSelected);
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (InventorySystem.Instance == null)
            return;

        InventoryItemData clickedItem = InventorySystem.Instance.GetItemAt(slotIndex);

        if (clickedItem == null)
            return;

        InventorySystem.Instance.SelectItem(slotIndex);
        RefreshUI();

        if (isSelectionMode && currentReceiver != null)
        {
            currentReceiver.OnItemSelectedFromInventory(clickedItem);
        }
    }

    public void OpenInventory()
    {
        if (!isOpen)
        {
            isOpen = true;
            SetInventoryVisiable(true);
            RefreshUI();
        }

    }

    public void CloseInventory()
    {
        if (!isOpen)
            return;

        isOpen = false;
        SetInventoryVisiable(false);
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        SetInventoryVisiable(isOpen);

        if (isOpen)
            RefreshUI();
    }

    public void OpenForItemSelection(IIventoryItemRecivers receiver)
    {
        currentReceiver = receiver;
        isSelectionMode = true;
        OpenInventory();
    }

    public void CloseSelectionMode()
    {
        currentReceiver = null;
        isSelectionMode = false;
        InventorySystem.Instance?.ClearSelection();
        RefreshUI();
        CloseInventory();
    }

    private void SetInventoryVisiable(bool visiable)
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(visiable);
        }
    }
}
