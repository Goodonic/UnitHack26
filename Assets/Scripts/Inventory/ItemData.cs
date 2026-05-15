using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Общая информация")]
    public string itemName;
    [TextArea(3, 10)]
    public string description;
    public Sprite icon;
    public int width;
    public int height;

    [Header("3D Визуал")]
    public GameObject itemPrefab;

    public enum ItemType
    {
        Food,
        Weapon,
        Resource,
        Quest
    }
    
    public ItemType type;
}
