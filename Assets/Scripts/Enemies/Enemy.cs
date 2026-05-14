using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyStats stats = new EnemyStats();

    public void Init(int hp, int damage)
    {
        stats.maxHP = hp;
        stats.currentHP = hp;
        stats.attackPower = damage;
    }

    public void TakeDamage(int dmg)
    {
        stats.TakeDamage(dmg);

        Debug.Log($"Enemy HP: {stats.currentHP}/{stats.maxHP}");

        if (stats.IsDead)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy died");
        CombatManager.Instance.EndCombat(true);
        Destroy(gameObject);
    }
}