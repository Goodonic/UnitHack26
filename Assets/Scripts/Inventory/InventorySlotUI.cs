using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IDropHandler
{
    [Tooltip("Порядковый номер этой ячейки в инвентаре (от 0 до 149)")]
    public int slotIndex;
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"На ячейку {slotIndex} что-то бросили!");

        InventoryItemUI draggedItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (draggedItem != null)
        {
            int fromIndex = draggedItem.originalSlotIndex;
            int toIndex = this.slotIndex;

            InventoryUI inventoryUI = FindAnyObjectByType<InventoryUI>();

            if (inventoryUI != null)
            {
                inventoryUI.inventoryLogic.SwapSlots(fromIndex, toIndex);
                inventoryUI.RefreshAll();
            }
            else
            {
                Debug.LogError("Скрипт InventoryUI не найден на сцене!");
            }
        }
    }
}
