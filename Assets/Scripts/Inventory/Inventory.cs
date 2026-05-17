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
        if (fromIndex < 0 || fromIndex >= TotalSlots || toIndex < 0 || toIndex >= TotalSlots) return;
        if (fromIndex == toIndex) return;

        InventorySlot fromSlot = slots[fromIndex];
        InventorySlot toSlot = slots[toIndex];

        bool fromIsEquip = IsEquipmentSlot(fromIndex);
        bool toIsEquip = IsEquipmentSlot(toIndex);

        // 1. Снимаем эффекты перед перемещением
        if (fromIsEquip && !fromSlot.IsEmpty) EquipmentManager.Instance.Unequip(fromSlot.itemData);
        if (toIsEquip && !toSlot.IsEmpty) EquipmentManager.Instance.Unequip(toSlot.itemData);

        // 2. Логика: Тянем СТАК из инвентаря в экипировку (отделяем 1 предмет)
        if (!fromIsEquip && toIsEquip && fromSlot.amount > 1)
        {
            if (toSlot.IsEmpty)
            {
                toSlot.itemData = fromSlot.itemData;
                toSlot.amount = 1;
                fromSlot.amount -= 1;

                EquipmentManager.Instance.Equip(toSlot.itemData);
                return;
            }
            else if (fromSlot.itemData.id != toSlot.itemData.id)
            {
                // Если в слоте экипировки лежит другой предмет, пробуем вернуть его в инвентарь
                InventorySlot emptyInInventory = FindEmptyInventorySlot();
                if (emptyInInventory != null)
                {
                    emptyInInventory.itemData = toSlot.itemData;
                    emptyInInventory.amount = toSlot.amount;

                    toSlot.itemData = fromSlot.itemData;
                    toSlot.amount = 1;
                    fromSlot.amount -= 1;

                    EquipmentManager.Instance.Equip(toSlot.itemData);
                    return;
                }
                else
                {
                    // Нет места в инвентаре для обмена шмотки — отмена
                    EquipmentManager.Instance.Equip(toSlot.itemData);
                    return;
                }
            }
            else
            {
                // Пытаемся положить тот же предмет в слот экипировки, где он уже есть — отмена
                EquipmentManager.Instance.Equip(toSlot.itemData);
                return;
            }
        }

        // 3. Логика: Возвращаем предмет из экипировки в инвентарь на СТАК такого же предмета
        if (fromIsEquip && !toIsEquip && !fromSlot.IsEmpty && !toSlot.IsEmpty)
        {
            if (fromSlot.itemData.id == toSlot.itemData.id && toSlot.itemData.isStackable)
            {
                int spaceLeft = toSlot.itemData.maxStackSize - toSlot.amount;
                if (spaceLeft >= fromSlot.amount)
                {
                    toSlot.amount += fromSlot.amount;
                    fromSlot.Clear();
                    // Слот очищен, бафф уже снят в шаге 1. Всё ок.
                    return;
                }
            }
        }

        // 4. Обычный полноценный обмен (работает и для перетаскивания НАЗАД из экипировки в пустой/занятый слот)
        InventorySlot temp = new InventorySlot();
        temp.itemData = fromSlot.itemData;
        temp.amount = fromSlot.amount;

        fromSlot.itemData = toSlot.itemData;
        fromSlot.amount = toSlot.amount;

        toSlot.itemData = temp.itemData;
        toSlot.amount = temp.amount;

        // 5. Применяем эффекты к тем предметам, которые оказались в слотах экипировки после обмена
        if (fromIsEquip && !fromSlot.IsEmpty) EquipmentManager.Instance.Equip(fromSlot.itemData);
        if (toIsEquip && !toSlot.IsEmpty) EquipmentManager.Instance.Equip(toSlot.itemData);
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
}
