using UnityEngine;

[System.Serializable]
public class EnemyAction
{
    public string actionName;
    public EnemyActionType type;
    public int value;

    [Tooltip("Шанс выбора. Чем больше число, тем выше вероятность.")]
    public int priority = 1;
}
public enum EnemyActionType
{
    Attack,
    Defend,
    Heal
}
