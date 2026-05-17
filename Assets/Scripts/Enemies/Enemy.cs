using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private class RuntimeAction
    {
        public EnemyAction data;
        public int currentColldown;
        public int timesUsed;
        public RuntimeAction(EnemyAction actionData)
        {
            data = actionData;
            currentColldown = 0;
            timesUsed = 0;
        }
        public bool IsAvailable =>
            currentColldown <= 0 &&
            (data.maxUsesPerCombat == 0 || timesUsed < data.maxUsesPerCombat);
    }

    public EnemyData Data { get; private set; }
    public int currentHp { get; private set; }
    public int currentBlock { get; private set; }

    private List<RuntimeAction> runtimeActions = new List<RuntimeAction>();

    public void Init(EnemyData enemyData)
    {
        Data = enemyData;
        currentHp = Data.maxHp;
        currentBlock = 0;

        runtimeActions.Clear();
        if(Data.possibleActions != null)
        {
            foreach (var action in Data.possibleActions)
            {
                runtimeActions.Add(new RuntimeAction(action));
            }
        }
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

    public void StartTurn()
    {
        currentBlock = 0; // Сбрасываем броню

        // Уменьшаем кулдаун у всех перезаряжающихся способностей
        foreach (var action in runtimeActions)
        {
            if (action.currentColldown > 0)
            {
                action.currentColldown--;
            }
        }
    }

    public void ResetBlock()
    {
        currentBlock = 0;
    }

    public void PerformRandomAction(PlayerCombat player)
    {
        // 1. Фильтруем только те действия, которые ДОСТУПНЫ
        List<RuntimeAction> availableActions = new List<RuntimeAction>();
        foreach (var action in runtimeActions)
        {
            if (action.IsAvailable)
            {
                availableActions.Add(action);
            }
        }

        // Если все способности на кулдауне или исчерпаны (защита от тупика)
        if (availableActions.Count == 0)
        {
            Debug.LogWarning($"{Data.enemyName} не имеет доступных ходов! Пропускает ход.");
            return;
        }

        // 2. Считаем общий вес только доступных действий
        int totalWeight = 0;
        foreach (var action in availableActions)
        {
            totalWeight += action.data.priority;
        }

        int randomValue = Random.Range(0, totalWeight);
        RuntimeAction selectedRuntimeAction = null;

        // 3. Выбираем действие по приоритету
        int currentWeight = 0;
        foreach (var action in availableActions)
        {
            currentWeight += action.data.priority;
            if (randomValue < currentWeight)
            {
                selectedRuntimeAction = action;
                break;
            }
        }

        if (selectedRuntimeAction == null) selectedRuntimeAction = availableActions[0];

        EnemyAction actualData = selectedRuntimeAction.data;
        Debug.Log($"--- {Data.enemyName} использует: {actualData.actionName} ---");

        // 4 Применяем
        switch (actualData.type)
        {
            case EnemyActionType.Attack:
                player.TakeDamage(actualData.value);
                break;

            case EnemyActionType.Defend:
                currentBlock += actualData.value;
                break;

            case EnemyActionType.Heal:
                currentHp += actualData.value;
                if (currentHp > Data.maxHp) currentHp = Data.maxHp;
                break;
        }

        // 5 Обновляем кулдаун и лимиты для использованной способности
        selectedRuntimeAction.timesUsed++;
        selectedRuntimeAction.currentColldown = actualData.cooldownTurns;

        if (actualData.cooldownTurns > 0)
        {
            Debug.Log($"-> Способность '{actualData.actionName}' уходит на перезарядку на {actualData.cooldownTurns} х.");
        }
        if (actualData.maxUsesPerCombat > 0)
        {
            Debug.Log($"-> Использовано {selectedRuntimeAction.timesUsed}/{actualData.maxUsesPerCombat} раз за бой.");
        }
    }
}