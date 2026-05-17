using UnityEngine;

[System.Serializable]
public class EnemyAction
{
    [Header("Basic")]
    public string actionName;
    public EnemyActionType type;
    public int value;

    [Tooltip("Шанс выбора. Чем больше число, тем выше вероятность.")]
    public int priority = 1;

    [Header("Cooldowns & Limits")]

    [Tooltip("Сколько ходов должно пройти перед повторным использованием (0 - можно каждый ход)")]
    public int cooldownTurns = 0;

    [Tooltip("Максимальное количество использований за один бой (0 - бесконечно)")]
    public int maxUsesPerCombat = 0;
}
public enum EnemyActionType
{
    Attack,
    Defend,
    Heal
}
