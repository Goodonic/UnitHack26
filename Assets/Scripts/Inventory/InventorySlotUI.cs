using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IDropHandler
{
    public int x, y;

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItemUI draggedItem = eventData.pointerDrag?.GetComponent<InventoryItemUI>();
        InventoryGrid grid = Object.FindAnyObjectByType<InventoryGrid>();

        if (draggedItem != null && grid != null)
        {
            grid.RemoveItem(draggedItem.itemData);

            if (grid.CanPlaceItem(draggedItem.itemData, this.x, this.y))
            {
                grid.PlaceItem(draggedItem.itemData, this.x, this.y);

                InventoryDisplay display = Object.FindAnyObjectByType<InventoryDisplay>();

                if (display != null)
                {
                    draggedItem.SetPosition(this.x, this.y, display.cellSize, display.spacing);
                }
                else
                {
                    draggedItem.SetPosition(this.x, this.y, 64f, 0f);
                }
            }
            else
            {
                Debug.Log("Места нет!");
            }
        }
    }
}
