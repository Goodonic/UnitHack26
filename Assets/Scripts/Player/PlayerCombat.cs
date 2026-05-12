using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public int maxHp = 50;
    public int currentHp;

    public int currentBlock;

    private void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(damage - currentBlock, 0);
        currentBlock = Mathf.Max(currentBlock - damage, 0);
        currentHp -= finalDamage;
        Debug.Log("Player HP: " + currentHp);
    }

    public void AddBlock(int amount)
    {
        currentBlock += amount;
        Debug.Log("Player Block: " + currentBlock);
    }

    public void ResetBlock()
    {
        currentBlock = 0;
    }
}