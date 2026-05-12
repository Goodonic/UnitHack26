using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public int maxHp = 20;
    public int currentHp;

    public int damage = 5;

    private void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int dmg)
    {
        currentHp -= dmg;
        Debug.Log("Enemy HP: " + currentHp);
    }

    public bool IsDead()
    {
        return currentHp <= 0;
    }

    public void Attack(PlayerCombat player)
    {
        player.TakeDamage(damage);
    }
}