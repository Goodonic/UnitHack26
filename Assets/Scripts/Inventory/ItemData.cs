 using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Идентификация")]
    [Tooltip("Уникальный ID предмета (item_sword_01 или просто число)")]
    public string id;

    [Header("Общая информация")]
    public string itemName;
    [TextArea(3, 10)]
    public string description;
    public Sprite icon;

    [Header("Настройки инвентаря")]
    [Tooltip("Можно ли собирать этот предмет в пачки?")]
    public bool isStackable;
    [Tooltip("Максимальное количество предметов в одной ячейке")]
    public int maxStackSize = 1;

    [Header("3D Визуал (для выпадания в мир)")]
    public GameObject itemPrefab;
}
