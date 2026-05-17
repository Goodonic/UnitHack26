using UnityEngine;
using Generation;
using UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveDelay = 0.2f;
    
    private Vector2Int currentPosition;
    private Direction currentDirection = Direction.North;
    private GridManager gridManager;
    private LevelBuilder levelBuilder;
    private DungeonGenerator dungeonGenerator;
    private CameraMotor cameraMotor;
    private PlayerLight playerLight;
    private bool isMoving = false;
    private bool inCombat = false;

    private PlayerInputActions inputActions;

    void Start()
    {
        gridManager = GridManager.Instance;
        levelBuilder = LevelBuilder.Instance;
        cameraMotor = GetComponent<CameraMotor>();
        playerLight = GetComponentInChildren<PlayerLight>();
        dungeonGenerator = DungeonGenerator.Instance;
        
        if (dungeonGenerator == null)
        {   
            dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        }
        
        FindStartPosition();
        UpdateCameraPosition();
        
        
        // Initialize input system
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        
        // Subscribe to input events
        inputActions.Player.MoveBackward.performed += ctx => TryMoveBackward();
        inputActions.Player.RotateLeft.performed += ctx => RotateLeft();
        inputActions.Player.RotateRight.performed += ctx => RotateRight();
    }
    
    void Update()
    {
        if (inputActions != null && inputActions.Player.MoveForward.IsPressed())
        {
            TryMoveForward();
        }
    }
    
    void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.MoveBackward.performed -= ctx => TryMoveBackward();
            inputActions.Player.RotateLeft.performed -= ctx => RotateLeft();
            inputActions.Player.RotateRight.performed -= ctx => RotateRight();
            
            inputActions.Disable();
            inputActions.Dispose();
        }
    }

    public void SetCombatState(bool state)
    {
        inCombat = state;
    }

    private void TryMoveForward()
    {
        if (isMoving || inCombat) return;

        Vector2Int targetPosition = currentPosition + currentDirection.ToVector();

        if (gridManager.IsValidPosition(targetPosition))
        {
            Tile targetTile = gridManager.GetTile(targetPosition);

            if (targetTile != null && targetTile.HasEnemy)
            {
                CombatManager.Instance.StartCombat(targetTile, targetTile.EnemyDataOnTile);
                return;
            }
            if (targetTile != null && targetTile.HasChest)
            {
                Debug.Log("Путь заблокирован сундуком!");
                return;
            }
        }

        if (CanMoveTo(targetPosition))
        {
            StartCoroutine(MoveToPosition(targetPosition));
        }
    }

    private void TryMoveBackward()
    {
        if (isMoving || inCombat) return;

        Direction opposite = GetOppositeDirection(currentDirection);
        Vector2Int targetPosition = currentPosition + opposite.ToVector();

        if (gridManager.IsValidPosition(targetPosition))
        {
            Tile targetTile = gridManager.GetTile(targetPosition);

            if (targetTile != null && (targetTile.HasEnemy || targetTile.HasChest))
            {
                Debug.Log("Назад идти нельзя — там препятствие/враг!");
                return;
            }
        }

        if (CanMoveTo(targetPosition))
        {
            StartCoroutine(MoveToPosition(targetPosition));
        }
    }

    private bool CanMoveTo(Vector2Int targetPosition)
    {
        if (!gridManager.IsValidPosition(targetPosition))
            return false;
            
        Tile currentTile = gridManager.GetTile(currentPosition);
        Direction moveDirection = GetDirectionToTarget(currentPosition, targetPosition);
        
        return !currentTile.HasWall(moveDirection);
    }
    
    private Direction GetDirectionToTarget(Vector2Int from, Vector2Int to)
    {
        Vector2Int delta = to - from;
        
        if (delta == new Vector2Int(0, 1)) return Direction.North;
        if (delta == new Vector2Int(0, -1)) return Direction.South;
        if (delta == new Vector2Int(1, 0)) return Direction.East;
        if (delta == new Vector2Int(-1, 0)) return Direction.West;
        
        return Direction.North;
    }
    
    private Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => Direction.North
        };
    }

    private System.Collections.IEnumerator MoveToPosition(Vector2Int targetPosition)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 targetPos = levelBuilder.GetCellWorldPosition(targetPosition);
        targetPos.y = transform.position.y;

        float elapsed = 0;
        while (elapsed < moveDelay)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDelay;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        currentPosition = targetPosition;

        CheckForExit();

        isMoving = false;
    }

    private void RotateLeft()
    {
        if (isMoving || inCombat || cameraMotor.IsRotating) return;

        currentDirection = currentDirection switch
        {
            Direction.North => Direction.West,
            Direction.West => Direction.South,
            Direction.South => Direction.East,
            Direction.East => Direction.North,
            _ => currentDirection
        };

        cameraMotor.RotateLeft();
    }

    private void RotateRight()
    {
        if (isMoving || inCombat || cameraMotor.IsRotating) return;

        currentDirection = currentDirection switch
        {
            Direction.North => Direction.East,
            Direction.East => Direction.South,
            Direction.South => Direction.West,
            Direction.West => Direction.North,
            _ => currentDirection
        };

        cameraMotor.RotateRight();
    }

    private void FindStartPosition()
    {
        Tile[,] grid = gridManager.GetGrid();

        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                if (grid[x, y].IsStart)
                {
                    currentPosition = new Vector2Int(x, y);
                    return;
                }
            }
        }

        // Если старт не найден — центр карты
        currentPosition = new Vector2Int(gridManager.Width / 2, gridManager.Height / 2);
    }

    private void UpdateCameraPosition()
    {
        Vector3 pos = levelBuilder.GetCellWorldPosition(currentPosition);
        pos.y = 1.7f;
        transform.position = pos;
        cameraMotor.SetRotation(currentDirection);

        if (MinimapRenderer.Instance != null)
        {
            MinimapRenderer.Instance.SetPlayerPosition(currentPosition);
        }
    }

    private void CheckForEnemy()
    {
        if (inCombat) return;

        Tile currentTile = gridManager.GetTile(currentPosition);

        if (currentTile == null) return;

        if (currentTile.HasEnemy)
        {
            CombatManager.Instance.StartCombat(currentTile, currentTile.EnemyDataOnTile);
        }
    }

    private void CheckForExit()
    {
        Tile currentTile = gridManager.GetTile(currentPosition);

        if (!currentTile.IsExit)
        {
            return;
        }

        if (dungeonGenerator == null)
        {
            Debug.Log("Dungeon generator not found.");
            return;
        }

        if (dungeonGenerator.TryGoToNextFloor())
        {
            FindStartPosition();
            UpdateCameraPosition();

            Debug.Log($"Level {dungeonGenerator.CurrentFloor} completed!");
        }
        else
        {
            Debug.Log("All floors completed!");
        }
    }
}