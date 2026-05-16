using UnityEngine;
using TMPro;

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

    [Header("Combat View")]
    [SerializeField] private float enemyDistance = 0.0f;
    [SerializeField] private float enemyHeightOffset = 2.0f;
    [SerializeField] private float enemyScale = 0.6f;

    private int currentStamina;

    private GameObject enemyVisual;
    private EnemyCombat enemy;

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

    public void StartCombat(Tile enemyTile)
    {
        if (isCombatActive) return;

        isCombatActive = true;
        player.SetCombatState(true);

        SetCombatUI(true);

        currentStamina = maxStamina;
        UpdateStaminaUI();

        turnState = TurnState.PlayerTurn;

        //пересбор руки каждый бой
        if (handUI != null)
        {
            handUI.DrawHand();
        }

        Debug.Log("COMBAT STARTED");

        Vector3 spawnPos = LevelBuilder.Instance.GetCellWorldPosition(enemyTile.Position);

        Vector3 lookDir = player.transform.forward;
        lookDir.y = 0f;
        lookDir.Normalize();

        enemyVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);

        enemyVisual.tag = "Enemy";

        enemyVisual.transform.position = spawnPos + lookDir * enemyDistance + Vector3.up * enemyHeightOffset;

        enemyVisual.transform.localScale = Vector3.one * enemyScale;

        enemyVisual.GetComponent<Renderer>().material.color = Color.red;

        enemy = enemyVisual.AddComponent<EnemyCombat>();
    }

    public void PlayCard(CardInstance card)
    {
        if (!isCombatActive || enemy == null) return;

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
                enemy.TakeDamage(card.data.value);

                if (enemy != null && enemy.IsDead())
                {
                    OnCardUsed?.Invoke(card);
                    EndCombat(true);
                    return;
                }
                break;

            case CardEffectType.Defend:
                playerCombat.AddBlock(card.data.value);
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
        if (enemy == null) return;

        enemy.Attack(playerCombat);
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
        if (enemy != null && enemy.IsDead())
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

        if (handUI != null)
        {
            handUI.ClearHand();
        }

        if (enemyVisual != null)
            Destroy(enemyVisual);

        enemy = null;
    }
}