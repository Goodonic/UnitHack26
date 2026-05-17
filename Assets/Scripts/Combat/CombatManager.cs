using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("State")]
    public bool isCombatActive;

    [Header("Stamina")]
    [SerializeField] private int maxStamina = 3;

    [Header("References")]
    public PlayerController player;
    public PlayerCombat playerCombat;

    [Header("UI")]
    [SerializeField] private GameObject handUIRoot;
    [SerializeField] private GameObject playZoneUI;

    [Header("Systems")]
    [SerializeField] private HandUI handUI;

    [Header("UI Stats")]
    [SerializeField] private TMP_Text staminaText;

    [Header("Loot System")]
    [SerializeField] private GameObject chestPrefab;

    private Tile currentEnemyTile;
    private EnemyData currentEnemyData; 

    [Header("Combat View")]
    [SerializeField] private float enemyDistance = 0.0f;
    [SerializeField] private float enemyHeightOffset = 2.0f;
    [SerializeField] private float enemyScale = 0.6f;

    private int currentStamina;

    private Enemy currentEnemy;

    public System.Action<CardInstance> OnCardUsed;

    public TurnState turnState;

    public bool IsPlayerTurn => turnState == TurnState.PlayerTurn;

    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }

    private void Start()
    {
        isCombatActive = false;
        SetCombatUI(false);
        UpdateStaminaUI();
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetCombatUI(false);
    }

    private void SetCombatUI(bool state)
    {
        if (handUIRoot != null)
            handUIRoot.SetActive(state);

        if (playZoneUI != null)
            playZoneUI.SetActive(state);
    }

    private void UpdateStaminaUI()
    {
        if (staminaText != null)
            staminaText.text = $"Stamina: {currentStamina}/{maxStamina}";
    }

    public void StartCombat(Tile enemyTile, EnemyData enemyDataToSpawn)
    {
        if (isCombatActive) return;

        currentEnemyTile = enemyTile;
        currentEnemyData = enemyDataToSpawn;

        isCombatActive = true;
        player.SetCombatState(true);
        SetCombatUI(true);

        currentStamina = maxStamina;
        UpdateStaminaUI();
        turnState = TurnState.PlayerTurn;

        if (EquipmentManager.Instance != null)
        {
            playerCombat.maxHp = 50 + EquipmentManager.Instance.bonusMaxHp;
            playerCombat.Heal(0);
        }

        if (handUI != null)
        {
            handUI.DrawHand();
        }

        Debug.Log($"COMBAT STARTED AGAINST: {enemyDataToSpawn.enemyName} (ID: {enemyDataToSpawn.enemyID})");

        Vector3 spawnPos = LevelBuilder.Instance.GetCellWorldPosition(enemyTile.Position);
        Vector3 lookDir = player.transform.forward;
        lookDir.y = 0f;
        lookDir.Normalize();

        Vector3 finalSpawnPos = spawnPos + lookDir * enemyDistance + Vector3.up * enemyHeightOffset;

        GameObject enemyVisual = Instantiate(enemyDataToSpawn.visualPrefab, finalSpawnPos, Quaternion.identity);
        enemyVisual.transform.localScale = Vector3.one * enemyScale;

        currentEnemy = enemyVisual.AddComponent<Enemy>();
        currentEnemy.Init(enemyDataToSpawn);
    }

    public void PlayCard(CardInstance card)
    {
        if (!isCombatActive || currentEnemy == null) return;

        if (!IsPlayerTurn)
        {
            Debug.Log("Not player turn");
            return;
        }

        if (card.data.cost > currentStamina)
        {
            Debug.Log("Not enough stamina");
            return;
        }

        currentStamina -= card.data.cost;
        UpdateStaminaUI();

        switch (card.data.effectType)
        {
            case CardEffectType.Attack:
                int finalDamage = card.data.value + (EquipmentManager.Instance != null ? EquipmentManager.Instance.bonusAttack : 0);

                currentEnemy.TakeDamage(finalDamage);
                Debug.Log($"Разыграна Атака! Базовый урон: {card.data.value} + Бонус экипировки: {(EquipmentManager.Instance != null ? EquipmentManager.Instance.bonusAttack : 0)} = Итоговый урон: {finalDamage}");

                if (currentEnemy != null && currentEnemy.IsDead)
                {
                    OnCardUsed?.Invoke(card);
                    EndCombat(true);
                    return;
                }
                break;

            case CardEffectType.Defend:
                int finalBlock = card.data.value + (EquipmentManager.Instance != null ? EquipmentManager.Instance.bonusDefense : 0);

                playerCombat.AddBlock(finalBlock);
                Debug.Log($"Разыграна Защита! Базовый блок: {card.data.value} + Бонус экипировки: {(EquipmentManager.Instance != null ? EquipmentManager.Instance.bonusDefense : 0)} = Итоговый блок: {finalBlock}");
                break;

            case CardEffectType.Heal:
                OnCardUsed?.Invoke(card);
                playerCombat.Heal(card.data.value);
                break;

            case CardEffectType.Weakness:
                OnCardUsed?.Invoke(card);
                Debug.Log("Weakness played");
                break;
        }

        if (currentStamina <= 0)
        {
            EndPlayerTurn();
        }
    }

    private void HandleCardConsumption(CardInstance card)
    {
        if (card.data.isConsumable)
        {
            Debug.Log($"Card consumed: {card.data.cardName}");
        }
    }

    private void StartPlayerTurn()
    {
        turnState = TurnState.PlayerTurn;
        currentStamina = maxStamina;

        UpdateStaminaUI();

        Debug.Log("Player Turn. Stamina: " + currentStamina);
    }

    private void EndPlayerTurn()
    {
        turnState = TurnState.EnemyTurn;
        Invoke(nameof(StartEnemyTurn), 0.5f);
    }

    private void StartEnemyTurn()
    {
        EnemyTurn();
    }

    private void EnemyTurn()
    {
        if (currentEnemy == null) return;

        currentEnemy.StartTurn();

        currentEnemy.PerformRandomAction(playerCombat);

        playerCombat.ResetBlock();

        if (playerCombat.currentHp <= 0)
        {
            Debug.Log("PLAYER LOST");
            EndCombat(false);
            return;
        }

        Invoke(nameof(EndEnemyTurn), 0.5f);
    }

    private void EndEnemyTurn()
    {
        StartPlayerTurn();
    }

    private bool CheckEnemyDeath()
    {
        if (currentEnemy != null && currentEnemy.IsDead)
        {
            EndCombat(true);
            return true;
        }

        return false;
    }

    public void EndCombat(bool playerWon)
    {
        isCombatActive = false;
        player.SetCombatState(false);
        SetCombatUI(false);

        currentStamina = 0;
        UpdateStaminaUI();

        if (playerWon && currentEnemyTile != null && currentEnemyData != null)
        {
            Debug.Log("ПОБЕДА! Враг повержен.");

            currentEnemyTile.EnemyDataOnTile = null;

            List<ItemData> generatedLoot = currentEnemyData.GenerateLoot();

            if (generatedLoot.Count > 0 && chestPrefab != null)
            {
                Vector3 spawnPos = LevelBuilder.Instance.GetCellWorldPosition(currentEnemyTile.Position);
                GameObject chestObj = Instantiate(chestPrefab, spawnPos, Quaternion.identity);

                Chest chest = chestObj.GetComponent<Chest>();
                if (chest != null)
                {
                    chest.InitChest(currentEnemyTile, generatedLoot);
                }
            }
        }

        if (currentEnemy != null)
            Destroy(currentEnemy.gameObject);

        currentEnemy = null;
        currentEnemyData = null;
    }
}