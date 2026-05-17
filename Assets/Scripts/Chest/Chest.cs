using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class Chest : MonoBehaviour
{
    private Inventory chestInventory;
    private Tile myTile;

    private void Awake()
    {
        chestInventory = GetComponent<Inventory>();
    }
    public void InitChest(Tile tile, List<ItemData> loot)
    {
        myTile = tile;
        if (myTile != null)
        {
            myTile.HasChest = true;
            myTile.ChestOnTile = this;
        }

        if (chestInventory == null) chestInventory = GetComponent<Inventory>();

        foreach (var slot in chestInventory.slots)
        {
            slot.Clear();
        }

        foreach (var item in loot)
        {
            chestInventory.AddItem(item, 1);
        }

        Debug.Log($"Сундук создан на клетке {myTile.Position}! Предметов: {loot.Count}");
    }

    public Inventory GetInventory()
    {
        return chestInventory;
    }

    public void DestroyChest()
    {
        if (myTile != null)
        {
            myTile.HasChest = false;
            myTile.EnemyDataOnTile = null;
            myTile.ChestOnTile = null;
        }

        Debug.Log("Сундук и зона врага уничтожены.");
        Destroy(gameObject);
    }
}
