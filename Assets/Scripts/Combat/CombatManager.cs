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
    public GameObject attackButton;
    public GameObject defendButton;

    [Header("Combat View")]
    [SerializeField] private float enemyDistance = 0.0f;
    [SerializeField] private float enemyHeightOffset = 2.0f;
    [SerializeField] private float enemyScale = 0.6f;

    private GameObject enemyVisual;
    private EnemyCombat enemy;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        attackButton.SetActive(false);
        defendButton.SetActive(false);
    }


    public void StartCombat(Tile enemyTile)
    {
        if (isCombatActive) return;

        isCombatActive = true;
        player.SetCombatState(true);
        attackButton.SetActive(true);
        defendButton.SetActive(true);

        Debug.Log("COMBAT STARTED");

        Vector3 spawnPos = LevelBuilder.Instance.GetCellWorldPosition(enemyTile.Position);

        Vector3 lookDir = player.transform.forward;
        lookDir.y = 0f;
        lookDir.Normalize();

        enemyVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemyVisual.transform.position = spawnPos + lookDir * enemyDistance + Vector3.up * enemyHeightOffset;
        enemyVisual.transform.localScale = Vector3.one * enemyScale;

        Renderer r = enemyVisual.GetComponent<Renderer>();
        r.material.color = Color.red;

        enemy = enemyVisual.AddComponent<EnemyCombat>();

        Debug.Log("Enemy spawned on tile");
    }

    public void PlayerAttack()
    {
        if (!isCombatActive) return;

        enemy.TakeDamage(6);

        CheckEnemyDeath();

        if (isCombatActive)
            EnemyTurn();
    }

    public void PlayerDefend()
    {
        if (!isCombatActive) return;
        playerCombat.AddBlock(5);
        EnemyTurn();
    }

    private void EnemyTurn()
    {
        enemy.Attack(playerCombat);
        playerCombat.ResetBlock();
    }

    private void CheckEnemyDeath()
    {
        if (enemy != null && enemy.IsDead())
        {
            EndCombat(true);
        }
    }

    public void EndCombat(bool playerWon)
    {
        isCombatActive = false;

        player.SetCombatState(false);
        attackButton.SetActive(false);
        defendButton.SetActive(false);

        Debug.Log("Combat finished. Player won: " + playerWon);

        if (enemyVisual != null)
            Destroy(enemyVisual);

        enemy = null;
    }
}