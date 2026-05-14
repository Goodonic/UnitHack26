using UnityEngine;

[System.Serializable]
public class EnemyStats
{
    public string enemyName = "Enemy";
    public int maxHP = 20;
    public int currentHP = 20;
    public int attackPower = 3;

    public bool IsDead => currentHP <= 0;

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0)
            currentHP = 0;
    }
}