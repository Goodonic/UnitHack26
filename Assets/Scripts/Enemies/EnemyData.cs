using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootDrop
{
    public ItemData item;
    [Tooltip("Шанс выпадения предмета (от 0 до 100%)")]
    [Range(0f, 100f)]
    public float dropChance;
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyID;
    public string enemyName;
    public GameObject visualPrefab;

    [Header("Stats")]
    public int maxHp;

    [Header("Arsenal")]
    public List<EnemyAction> possibleActions;

    [Header("Loot Settings")]
    public int minDrops = 1;
    public int maxDrops = 4;

    public List<LootDrop> lootTable;

    public List<ItemData> GenerateLoot()
    {
        List<ItemData> droppedItems = new List<ItemData>();

        int targetDropCount = Random.Range(minDrops, maxDrops + 1);

        List<LootDrop> shuffledLoot = new List<LootDrop>(lootTable);
        for (int i = 0; i < shuffledLoot.Count; i++)
        {
            LootDrop temp = shuffledLoot[i];
            int randomIndex = Random.Range(i, shuffledLoot.Count);
            shuffledLoot[i] = shuffledLoot[randomIndex];
            shuffledLoot[randomIndex] = temp;
        }

        foreach (var drop in shuffledLoot)
        {
            if (droppedItems.Count >= targetDropCount) break;

            float roll = Random.Range(0f, 100f);

            if (roll <= drop.dropChance)
            {
                droppedItems.Add(drop.item);
            }
        }

        return droppedItems;
    }
}
