using UnityEngine;

public class InventoryGrid : MonoBehaviour
{
    public const int GridWidth = 8;
    public const int GridHeight = 8;

    private ItemData[,] grid = new ItemData[GridWidth, GridHeight];

    public bool CanPlaceItem(ItemData item, int x, int y)
    {
        if(x<0 || y < 0 || x + item.width > GridWidth || y + item.height > GridHeight)
            return false;
        for(int i = x; i < x + item.width; i++)
        {
            for(int j = y;  j < y + item.height; j++)
            {
                if (grid[i, j] != null) return false;
            }
        }
        return true;
    }
    public bool PlaceItem(ItemData item, int x, int y)
    {
        if(!CanPlaceItem(item, x, y)) return false;

        for (int i = x; i < x + item.width; i++)
        {
            for (int j = y; j < y + item.height; j++)
            {
                grid[i, j] = item;
            }
        }
        Debug.Log($"Предмет {item.itemName} размещен в {x}:{y}");
        return true;
    }
    public void RemoveItem(ItemData item)
    {
        for (int x  = 0; x < GridWidth; x++)
        {
            for (int  y = 0; y < GridHeight; y++)
            {
                if (grid[x, y] == item) grid[x, y] = null;
            }
        }
    }
}
