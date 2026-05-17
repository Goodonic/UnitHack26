using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    [Header("Настройки инвентаря")]
    public int columns = 5;
    public int rows = 30;

    public int TotalSlots => columns * rows;

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
    public void SwapSlots(int index1, int index2)
    {
        Debug.Log($"Логика: Меняем местами ячейки {index1} и {index2}");
        if (index1 < 0 || index1 >= TotalSlots || index2 < 0 || index2 >= TotalSlots)
            return;
        if (index1 == index2) return;

        InventorySlot slot1 = slots[index1];
        InventorySlot slot2 = slots[index2];

        if (!slot1.IsEmpty && !slot2.IsEmpty &&
        slot1.itemData.id == slot2.itemData.id && slot1.itemData.isStackable)
        {
            int spaceLeft = slot2.itemData.maxStackSize - slot2.amount;

            if (spaceLeft > 0)
            {
                int amountToMove = Mathf.Min(slot1.amount, spaceLeft);
                slot2.amount += amountToMove;
                slot1.amount -= amountToMove;

                if (slot1.amount <= 0) slot1.Clear();

                return;
            }
        }

        InventorySlot temp = new InventorySlot();
        temp.itemData = slot1.itemData;
        temp.amount = slot1.amount;

        slot1.itemData = slot2.itemData;
        slot1.amount = slot2.amount;

        slot2.itemData = temp.itemData;
        slot2.amount = temp.amount;
    }
    public bool AddItem(ItemData itemData, int amount)
    {
        if (itemData.isStackable)
        {
            foreach(var slot in slots)
            {
                if(!slot.IsEmpty && slot.itemData.id == itemData.id && slot.amount < itemData.maxStackSize)
                {
                    int addedAmount = Mathf.Min(amount, itemData.maxStackSize - slot.amount);
                    slot.amount += addedAmount;
                    amount -= addedAmount;

                    if(amount <= 0) return true;
                }
            }
        }
        while (amount > 0)
        {
            InventorySlot emptySlot = FindEmptySlot();
            if(emptySlot ==  null) return false;

            emptySlot.itemData = itemData;
            int add = Mathf.Min(amount, itemData.maxStackSize);
            emptySlot.amount = add;
            amount -= add;

        }
        return true;
    }
    private InventorySlot FindEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty) return slot;
        }
        return null;
    }
}
