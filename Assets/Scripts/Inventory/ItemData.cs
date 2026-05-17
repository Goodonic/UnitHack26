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

    [Header("Бонусы экипировки")]
    [Tooltip("Бонус к урону для атакующих карт")]
    public int attackBonus;
    [Tooltip("Бонус к защите для карт защиты")]
    public int defenseBonus;
    [Tooltip("Бонус к максимальному и текущему здоровью игрока")]
    public int hpBonus;

    [Header("Связь с картами")]
    [Tooltip("Карта, которая дается при надевании. Оставь пустым, если дает только баффы.")]
    public CardData grantedCard;

    [Header("3D Визуал (для выпадания в мир)")]
    public GameObject itemPrefab;
}
