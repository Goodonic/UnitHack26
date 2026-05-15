using UnityEngine;
using UnityEngine.UI;
public class InventoryDisplay : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform backgroundGrid;
    public Transform itemContainer;

    [Header("Префабы")]
    public GameObject slotPrefab;
    public GameObject itemUIPrefab;

    [Header("Параметры сетки")]
    public int rows = 8;
    public int columns = 8;
    public float cellSize = 64f;
    public float spacing = 1f; 



    [Header("Тестирование")]
    public ItemData startItem;

    void Start()
    {
        GenerateBackgroundGrid();

        if (startItem != null)
        {
            SpawnItemUI(startItem, 0, 0);
        }
    }
    void GenerateBackgroundGrid()
    {
        int index = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject slot = Instantiate(slotPrefab, backgroundGrid);
                InventorySlotUI slotUI = slot.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.x = x;
                    slotUI.y = y;
                }
            }
        }
    }
    public void SpawnItemUI(ItemData item, int x, int y)
    {
        GameObject obj = Instantiate(itemUIPrefab, itemContainer);
        InventoryItemUI itemUI = obj.GetComponent<InventoryItemUI>();

        if (itemUI != null)
        {
            itemUI.SetItem(item, cellSize, spacing);
            itemUI.SetPosition(x, y, cellSize, spacing);
        }
    }
}

