using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCombat : MonoBehaviour
{
    [Header("HP")]
    public int maxHp = 50;
    public int currentHp;

    [Header("Block")]
    public int currentBlock;

    [Header("UI")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private TMP_Text hpText;

    private void Awake()
    {
        currentHp = maxHp;
    }

    private void Start()
    {
        UpdateHpUI();
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(damage - currentBlock, 0);

        currentBlock = Mathf.Max(currentBlock - damage, 0);

        currentHp -= finalDamage;
        currentHp = Mathf.Max(currentHp, 0);

        UpdateHpUI();

        Debug.Log("Player HP: " + currentHp);
    }

    public void AddBlock(int amount)
    {
        currentBlock += amount;

        Debug.Log("Player Block: " + currentBlock);
    }

    public void Heal(int amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);

        UpdateHpUI();

        Debug.Log("Player HP: " + currentHp);
    }

    public void ResetBlock()
    {
        currentBlock = 0;
    }

    private void UpdateHpUI()
    {
        if (hpBar != null)
        {
            hpBar.maxValue = maxHp;
            hpBar.value = currentHp;
        }

        if (hpText != null)
        {
            hpText.text = currentHp + "";
        }
    }
}