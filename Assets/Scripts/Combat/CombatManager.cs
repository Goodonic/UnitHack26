using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("State")]
    public bool isCombatActive;

    [Header("References")]
    public PlayerController player;
    public PlayerCombat playerCombat;

    [Header("UI")]
    [SerializeField] private GameObject handUI;
    [SerializeField] private GameObject playZoneUI;

    [Header("Combat View")]
    [SerializeField] private float enemyDistance = 0.0f;
    [SerializeField] private float enemyHeightOffset = 2.0f;
    [SerializeField] private float enemyScale = 0.6f;

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
        if (handUI != null)
            handUI.SetActive(state);

        if (playZoneUI != null)
            playZoneUI.SetActive(state);
    }

    public void StartCombat(Tile enemyTile)
    {
        if (isCombatActive) return;

        isCombatActive = true;
        player.SetCombatState(true);
        SetCombatUI(true);

        Debug.Log("COMBAT STARTED");

        turnState = TurnState.PlayerTurn;

        Vector3 spawnPos = LevelBuilder.Instance.GetCellWorldPosition(enemyTile.Position);

        Vector3 lookDir = player.transform.forward;
        lookDir.y = 0f;
        lookDir.Normalize();

        enemyVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemyVisual.transform.position =
            spawnPos + lookDir * enemyDistance + Vector3.up * enemyHeightOffset;

        enemyVisual.transform.localScale = Vector3.one * enemyScale;

        Renderer r = enemyVisual.GetComponent<Renderer>();
        r.material.color = Color.red;

        enemy = enemyVisual.AddComponent<EnemyCombat>();

        Debug.Log("Enemy spawned");
    }

    public void PlayCard(CardInstance card)
    {
        if (!isCombatActive || enemy == null) return;

        if (!IsPlayerTurn)
        {
            Debug.Log("Not player turn");
            return;
        }

        switch (card.data.effectType) //базовые атака/защита не пропадают
        {
            case CardEffectType.Attack:
                enemy.TakeDamage(card.data.value);

                if (enemy != null && enemy.IsDead())
                {
                    //OnCardUsed?.Invoke(card);
                    EndCombat(true);
                    return;
                }

                //OnCardUsed?.Invoke(card);
                EndPlayerTurn();
                break;

            case CardEffectType.Defend:
                playerCombat.AddBlock(card.data.value);

                //OnCardUsed?.Invoke(card);
                EndPlayerTurn();
                break;

            case CardEffectType.Heal:
                playerCombat.Heal(card.data.value);

                OnCardUsed?.Invoke(card);
                EndPlayerTurn();
                break;

            case CardEffectType.Weakness:
                Debug.Log("Weakness played");

                OnCardUsed?.Invoke(card);
                EndPlayerTurn();
                break;
        }
    }

    private void HandleCardConsumption(CardInstance card)
    {
        if (card.data.isConsumable)
        {
            Debug.Log($"Card consumed: {card.data.cardName}");
        }
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
        turnState = TurnState.PlayerTurn;
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

        Debug.Log("Combat finished. Player won: " + playerWon);

        if (enemyVisual != null)
            Destroy(enemyVisual);

        enemy = null;
    }
}