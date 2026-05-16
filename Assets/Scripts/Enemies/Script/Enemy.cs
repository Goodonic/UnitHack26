using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData Data { get; private set; }
    public int currentHp { get; private set; }
    public int currentBlock { get; private set; }

    public void Init(EnemyData enemyData)
    {
        Data = enemyData;
        currentHp = Data.maxHp;
        currentBlock = 0;
    }

    public void TakeDamage(int dmg)
    {
        if (currentBlock > 0)
        {
            if (dmg >= currentBlock)
            {
                dmg -= currentBlock;
                currentBlock = 0;
            }
            else
            {
                currentBlock -= dmg;
                dmg = 0;
            }
        }
        currentHp -= dmg;
        if (currentHp < 0) currentHp = 0;

        Debug.Log($"{Data.enemyName} HP: {currentHp}/{Data.maxHp} | Block: {currentBlock}");
    }

    public bool IsDead => currentHp <= 0;

    public void ResetBlock()
    {
        currentBlock = 0;
    }

    public void PerformRandomAction(PlayerCombat player)
    {
        if (Data.possibleActions == null || Data.possibleActions.Count == 0)
        {
            Debug.LogWarning($"У {Data.enemyName} нет доступных действий!");
            return;
        }

        // 1. Считаем общий вес (сумму всех приоритетов)
        int totalWeight = 0;
        foreach (var action in Data.possibleActions)
        {
            totalWeight += action.priority;
        }

        // 2. Кидаем кубик от 0 до (totalWeight - 1)
        int randomValue = Random.Range(0, totalWeight);
        EnemyAction selectedAction = null;

        int currentWeight = 0;
        foreach (var action in Data.possibleActions)
        {
            currentWeight += action.priority;
            if (randomValue < currentWeight)
            {
                selectedAction = action;
                break;
            }
        }
        if (selectedAction == null)
        {
            selectedAction = Data.possibleActions[0];
        }

        Debug.Log($"--- {Data.enemyName} использует: {selectedAction.actionName} (Приоритет: {selectedAction.priority}) ---");

        // 3. Применяем эффект
        switch (selectedAction.type)
        {
            case EnemyActionType.Attack:
                player.TakeDamage(selectedAction.value);
                Debug.Log($"-> Нанесено {selectedAction.value} урона игроку.");
                break;

            case EnemyActionType.Defend:
                currentBlock += selectedAction.value;
                Debug.Log($"-> Получено {selectedAction.value} брони.");
                break;

            case EnemyActionType.Heal:
                currentHp += selectedAction.value;
                if (currentHp > Data.maxHp) currentHp = Data.maxHp;
                Debug.Log($"-> Восстановлено {selectedAction.value} ХП. Теперь ХП: {currentHp}");
                break;
        }
    }
}