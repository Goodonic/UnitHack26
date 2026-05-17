using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int amount;

    //проверка на пустоту ячейки
    public bool IsEmpty => itemData == null || amount == 0;

    //Конструктор по умолчанию(создает пустую ячейку)
    public InventorySlot()
    {
        itemData = null;
        amount = 0;
    }

    public void Clear()
    {
        itemData = null;
        amount = 0;
    }

}
