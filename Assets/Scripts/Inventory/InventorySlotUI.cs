using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image slotBackground;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image selectedHighlight;

    private int slotIndex = -1;
    private bool isEmpty = true;
    private InventoryUIController ownerUI;

    private void Reset()
    {
        slotBackground = GetComponent<Image>();
    }

    public void Initialize(InventoryUIController uiController, int index)
    {
        ownerUI = uiController;
        slotIndex = index;
    }

    public void SetEmpty()
    {
        isEmpty = true;

        if(itemIcon != null)
        {
            itemIcon.enabled = false;
            itemIcon.sprite = null;
        }
    }

    public void SetItem(Sprite iconsprite)
    {
        isEmpty = false;

        if (itemIcon != null)
        {
            itemIcon.enabled = true;
            itemIcon.sprite = iconsprite;
        }
    }

    private void SetSelected(bool selected)
    {
        if (selectedHighlight != null)
        {
            selectedHighlight.enabled = selected;
        }
    }

    public bool IsEmpty()
    {
        return isEmpty;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ownerUI == null)
        {
            return;
        }

        ownerUI.OnSlotClicked(slotIndex);
    }

}
