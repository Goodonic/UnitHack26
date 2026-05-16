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

            // ИЗМЕНЕНИЕ ЗДЕСЬ: Ищем InventoryUI во всей сцене, а не только у родителей
            InventoryUI inventoryUI = FindAnyObjectByType<InventoryUI>();

            if (inventoryUI != null)
            {
                // Если менеджер найден, запускаем обмен и обновляем визуал
                inventoryUI.inventoryLogic.SwapSlots(fromIndex, toIndex);
                inventoryUI.RefreshAll();
            }
            else
            {
                // Добавил ошибку на всякий случай, чтобы мы точно знали, если он опять потеряется
                Debug.LogError("Скрипт InventoryUI не найден на сцене!");
            }
        }
    }
}
