using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    [Header("Настройки инвентаря")]
    public int columns = 5;
    public int rows = 28;

    [Header("Настройки экипировки")]
    public int equipmentSlotsCount = 6;

    public int TotalInventorySlots => columns * rows;
    public int TotalSlots => TotalInventorySlots + equipmentSlotsCount;

    [Header("Содержимое")]
    public InventorySlot[] slots;

    private void Awake()
    {
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        slots = new InventorySlot[TotalSlots];
        for (int i = 0; i < TotalSlots; i++)
        {
            slots[i] = new InventorySlot();
        }
    }

    // Проверка: является ли этот индекс ячейкой экипировки?
    public bool IsEquipmentSlot(int index)
    {
        return index >= TotalInventorySlots && index < TotalSlots;
    }

    public void SwapSlots(int fromIndex, int toIndex)
    {
        // Проверка на корректность индексов
        if (fromIndex < 0 || fromIndex >= slots.Length || toIndex < 0 || toIndex >= slots.Length) return;
        if (fromIndex == toIndex) return;

        InventorySlot fromSlot = slots[fromIndex];
        InventorySlot toSlot = slots[toIndex];

        if (fromSlot == null || fromSlot.IsEmpty) return;

        bool fromIsEquip = IsEquipmentSlot(fromIndex);
        bool toIsEquip = IsEquipmentSlot(toIndex);

        // 1. УМНОЕ ОБЪЕДИНЕНИЕ С УЧЕТОМ MAX STACK SIZE
        if (toSlot != null && !toSlot.IsEmpty && fromSlot.itemData == toSlot.itemData)
        {
            // Проверяем, разрешено ли предмету стакаться, и что мы не пытаемся стакать внутри экипировки
            if (fromSlot.itemData.isStackable && !toIsEquip)
            {
                int maxStack = fromSlot.itemData.maxStackSize;

                // Если в целевом слоте еще есть место
                if (toSlot.amount < maxStack)
                {
                    int spaceLeft = maxStack - toSlot.amount;
                    int amountToMove = Mathf.Min(fromSlot.amount, spaceLeft); 

                    toSlot.amount += amountToMove;
                    fromSlot.amount -= amountToMove;

                    if (fromSlot.amount <= 0)
                    {
                        if (fromIsEquip && EquipmentManager.Instance != null)
                            EquipmentManager.Instance.Unequip(fromSlot.itemData);

                        fromSlot.itemData = null;
                        fromSlot.amount = 0;
                    }

                    return; 
                }
            }
        }

        // 2. ЗАЩИТА СПЕЦИАЛЬНЫХ ЯЧЕЕК ОТ СТАКОВ ПРИ ОБМЕНЕ

        if (fromIsEquip && toSlot != null && !toSlot.IsEmpty && toSlot.amount > 1)
        {
            Debug.LogWarning("Нельзя поменять экипированную вещь местами со стаком предметов!");
            return;
        }

        if (toIsEquip && fromSlot.amount > 1)
        {
            if (toSlot == null || toSlot.IsEmpty)
            {
                toSlot.itemData = fromSlot.itemData;
                toSlot.amount = 1;
                fromSlot.amount--;

                if (EquipmentManager.Instance != null)
                    EquipmentManager.Instance.Equip(toSlot.itemData);
                return;
            }
            else
            {
                Debug.LogWarning("Нельзя засунуть стак предметов в занятый слот экипировки!");
                return;
            }
        }

        // 3. ОБЫЧНЫЙ БЕЗОПАСНЫЙ ОБМЕН (СВАП) ПРЕДМЕТОВ

        // Снимаем шмотки перед обменом
        if (fromIsEquip && !fromSlot.IsEmpty && EquipmentManager.Instance != null)
            EquipmentManager.Instance.Unequip(fromSlot.itemData);

        if (toIsEquip && toSlot != null && !toSlot.IsEmpty && EquipmentManager.Instance != null)
            EquipmentManager.Instance.Unequip(toSlot.itemData);

        // Стандартный алгоритм обмена данными
        ItemData tempItem = fromSlot.itemData;
        int tempAmount = fromSlot.amount;

        fromSlot.itemData = toSlot.itemData;
        fromSlot.amount = toSlot.amount;

        toSlot.itemData = tempItem;
        toSlot.amount = tempAmount;

        // Надеваем шмотки обратно в новые слоты
        if (fromIsEquip && !fromSlot.IsEmpty && EquipmentManager.Instance != null)
            EquipmentManager.Instance.Equip(fromSlot.itemData);

        if (toIsEquip && !toSlot.IsEmpty && EquipmentManager.Instance != null)
            EquipmentManager.Instance.Equip(toSlot.itemData);
    }

    public bool AddItem(ItemData itemData, int amount)
    {
        if (itemData.isStackable)
        {
            for (int i = 0; i < TotalInventorySlots; i++)
            {
                InventorySlot slot = slots[i];
                if (!slot.IsEmpty && slot.itemData.id == itemData.id && slot.amount < itemData.maxStackSize)
                {
                    int addedAmount = Mathf.Min(amount, itemData.maxStackSize - slot.amount);
                    slot.amount += addedAmount;
                    amount -= addedAmount;

                    if (amount <= 0) return true;
                }
            }
        }

        while (amount > 0)
        {
            InventorySlot emptySlot = FindEmptyInventorySlot();
            if (emptySlot == null) return false;

            emptySlot.itemData = itemData;
            int add = Mathf.Min(amount, itemData.maxStackSize);
            emptySlot.amount = add;
            amount -= add;
        }
        return true;
    }

    private InventorySlot FindEmptyInventorySlot()
    {
        for (int i = 0; i < TotalInventorySlots; i++)
        {
            if (slots[i].IsEmpty) return slots[i];
        }
        return null;
    }

    public void MoveItemToOtherInventory(int sourceIndex, Inventory targetInventory, int targetIndex)
    {
        if (sourceIndex < 0 || sourceIndex >= slots.Length) return;
        if (targetInventory == null) return;
        if (targetIndex < 0 || targetIndex >= targetInventory.slots.Length) return;

        InventorySlot fromSlot = slots[sourceIndex];
        InventorySlot toSlot = targetInventory.slots[targetIndex];

        if (fromSlot == null || fromSlot.IsEmpty) return;

        bool fromIsEquip = IsEquipmentSlot(sourceIndex);
        bool toIsEquip = targetInventory.IsEquipmentSlot(targetIndex);

        // 1. УМНОЕ ОБЪЕДИНЕНИЕ (СТАКИНГ) МЕЖДУ РАЗНЫМИ ИНВЕНТАРЯМИ
        if (toSlot != null && !toSlot.IsEmpty && fromSlot.itemData == toSlot.itemData)
        {
            if (fromSlot.itemData.isStackable && !toIsEquip)
            {
                int maxStack = fromSlot.itemData.maxStackSize;

                if (toSlot.amount < maxStack)
                {
                    int spaceLeft = maxStack - toSlot.amount;
                    int amountToMove = Mathf.Min(fromSlot.amount, spaceLeft);

                    toSlot.amount += amountToMove;
                    fromSlot.amount -= amountToMove;

                    if (fromSlot.amount <= 0)
                    {
                        if (fromIsEquip && EquipmentManager.Instance != null)
                            EquipmentManager.Instance.Unequip(fromSlot.itemData);

                        fromSlot.itemData = null;
                        fromSlot.amount = 0;
                    }
                    return;
                }
            }
        }

        // 2. ОГРАНИЧЕНИЯ ДЛЯ СЛОТОВ ЭКИПИРОВКИ В ЦЕЛЕВОМ ИНВЕНТАРЕ
        if (toIsEquip && fromSlot.amount > 1)
        {
            Debug.LogWarning("Нельзя переместить стак предметов сразу в слот экипировки!");
            return;
        }

        // 3. ПОЛНЫЙ ПЕРЕНОС ИЛИ СВАП ПРЕДМЕТОВ МЕЖДУ РАЗНЫМИ ИНВЕНТАРЯМИ
        if (fromIsEquip && EquipmentManager.Instance != null)
            EquipmentManager.Instance.Unequip(fromSlot.itemData);

        if (toIsEquip && !toSlot.IsEmpty && EquipmentManager.Instance != null)
            EquipmentManager.Instance.Unequip(toSlot.itemData);

        ItemData tempItem = fromSlot.itemData;
        int tempAmount = fromSlot.amount;

        fromSlot.itemData = toSlot.itemData;
        fromSlot.amount = toSlot.amount;

        toSlot.itemData = tempItem;
        toSlot.amount = tempAmount;

        if (fromIsEquip && !fromSlot.IsEmpty && EquipmentManager.Instance != null)
            EquipmentManager.Instance.Equip(fromSlot.itemData);

        if (toIsEquip && !toSlot.IsEmpty && EquipmentManager.Instance != null)
            EquipmentManager.Instance.Equip(toSlot.itemData);
    }
}
