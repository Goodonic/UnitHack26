using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Текущие бонусы")]
    public int bonusAttack = 0;
    public int bonusDefense = 0;
    public int bonusMaxHp = 0;

    [Header("Ссылки на системы")]
    public PlayerCombat playerCombat;

    private List<ItemData> equippedItems = new List<ItemData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (playerCombat == null) playerCombat = FindAnyObjectByType<PlayerCombat>();
    }

    public void Equip(ItemData item)
    {
        if (item == null) return;

        equippedItems.Add(item);

        bonusAttack += item.attackBonus;
        bonusDefense += item.defenseBonus;
        bonusMaxHp += item.hpBonus;

        if (playerCombat != null && item.hpBonus > 0)
        {
            playerCombat.maxHp += item.hpBonus;
            playerCombat.Heal(item.hpBonus);
        }

        Debug.Log($"Надето: {item.itemName}. Бонусы -> Атака: +{bonusAttack}, Защита: +{bonusDefense}, ХП: +{bonusMaxHp}");
    }

    public void Unequip(ItemData item)
    {
        if (item == null) return;

        if (equippedItems.Contains(item))
        {
            equippedItems.Remove(item);
        }

        bonusAttack -= item.attackBonus;
        bonusDefense -= item.defenseBonus;
        bonusMaxHp -= item.hpBonus;

        if (playerCombat != null && item.hpBonus > 0)
        {
            playerCombat.maxHp -= item.hpBonus;
            if (playerCombat.currentHp > playerCombat.maxHp)
            {
                playerCombat.currentHp = playerCombat.maxHp;
            }
            playerCombat.TakeDamage(0); 
        }

        Debug.Log($"Снято: {item.itemName}. Бонусы -> Атака: +{bonusAttack}, Защита: +{bonusDefense}, ХП: +{bonusMaxHp}");
    }

    public List<CardData> GetEquipmentCards()
    {
        List<CardData> equipmentCards = new List<CardData>();

        foreach (var item in equippedItems)
        {
            if (item != null && item.grantedCard != null)
            {
                equipmentCards.Add(item.grantedCard);
            }
        }

        return equipmentCards;
    }
}
