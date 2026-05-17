using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class Chest : MonoBehaviour
{
    [Header("References")]
    private Inventory chestInventory;

    private void Awake()
    {
        chestInventory = GetComponent<Inventory>();
    }
    public void InitChest(List<ItemData> loot)
    {
        if (chestInventory == null) chestInventory = GetComponent<Inventory>();

        foreach (var slot in chestInventory.slots)
        {
            slot.Clear();
        }

        foreach (var item in loot)
        {
            chestInventory.AddItem(item, 1);
        }

        Debug.Log($"Сундук создан! Предметов внутри: {loot.Count}");
    }

    public Inventory GetInventory()
    {
        return chestInventory;
    }

    public void DestroyChest()
    {
        // Добавить анимацию
        Debug.Log("Сундук исчезает с карты.");
        Destroy(gameObject);
    }
}
