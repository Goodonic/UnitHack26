using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveDelay = 0.2f;
    
    private Vector2Int currentPosition;
    private Direction currentDirection = Direction.North;
    private GridManager gridManager;
    private LevelBuilder levelBuilder;
    private CameraMotor cameraMotor;
    private bool isMoving = false;
    
    void Start()
    {
        gridManager = GridManager.Instance;
        levelBuilder = LevelBuilder.Instance;
        cameraMotor = GetComponent<CameraMotor>();
        
        FindStartPosition();
        UpdateCameraPosition();
    }
    
    void Update()
    {
        if (isMoving) return;
        
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            TryMoveForward();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            TryMoveBackward();
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RotateLeft();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            RotateRight();
        }
    }
    
    private void TryMoveForward()
    {
        Vector2Int targetPosition = currentPosition + currentDirection.ToVector();
        
        if (CanMoveTo(targetPosition))
        {
            StartCoroutine(MoveToPosition(targetPosition));
        }
    }
    
    private void TryMoveBackward()
    {
        Direction opposite = GetOppositeDirection(currentDirection);
        Vector2Int targetPosition = currentPosition + opposite.ToVector();
        
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
        
        // Если старт не найден, начинаем с центра
        currentPosition = new Vector2Int(gridManager.Width / 2, gridManager.Height / 2);
    }
    
    private void UpdateCameraPosition()
    {
        Vector3 pos = levelBuilder.GetCellWorldPosition(currentPosition);
        pos.y = 1.7f;
        transform.position = pos;
        cameraMotor.SetRotation(currentDirection);
    }
    
    private void CheckForExit()
    {
        Tile currentTile = gridManager.GetTile(currentPosition);
        if (currentTile.IsExit)
        {
            Debug.Log("Level Complete!");
            // TODO Логика завершения уровня
        }
    }
}