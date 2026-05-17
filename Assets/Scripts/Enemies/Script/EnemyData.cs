using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyID;
    public string enemyName;

    [Header("Stats")]
    public int maxHp;

    [Header("Arsenal")]
    public List<EnemyAction> possibleActions;

    [Header("Visuals")]
    public GameObject visualPrefab;
}
