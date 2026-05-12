using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public static LevelBuilder Instance { get; private set; }
    [Header("Size")]
    [SerializeField] private float CELL_SIZE = 3f;
    [SerializeField] private float WALL_HEIGHT = 3f;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    
    [Header("Materials")]
    [SerializeField] private Material stoneMaterial;
    [SerializeField] private Material dirtMaterial;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private Material wallMaterial;
    
    [Header("Colors (fallback)")]
    [SerializeField] private Color stoneColor = Color.gray;
    [SerializeField] private Color dirtColor = new Color(0.55f, 0.41f, 0.27f);
    [SerializeField] private Color waterColor = Color.blue;
    [SerializeField] private Color wallColor = Color.black;
    
    
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void BuildLevel(Tile[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        
        GameObject levelParent = new GameObject("Level");
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = grid[x, y];
                Vector3 worldPos = new Vector3(x * CELL_SIZE, 0, y * CELL_SIZE);
                
                BuildFloor(tile, worldPos, levelParent.transform);
                BuildWalls(tile, worldPos, levelParent.transform);
            }
        }
    }
    
    private void BuildFloor(Tile tile, Vector3 position, Transform parent)
    {
        GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity, parent);
        floor.transform.localScale = new Vector3(CELL_SIZE, 0.1f, CELL_SIZE);
        
        Renderer renderer = floor.GetComponent<Renderer>();
        if (renderer != null)
        {
            switch (tile.GroundType)
            {
                case TileType.Stone:
                    if (stoneMaterial != null) renderer.material = stoneMaterial;
                    else renderer.material.color = stoneColor;
                    break;
                case TileType.Dirt:
                    if (dirtMaterial != null) renderer.material = dirtMaterial;
                    else renderer.material.color = dirtColor;
                    break;
                case TileType.Water:
                    if (waterMaterial != null) renderer.material = waterMaterial;
                    else renderer.material.color = waterColor;
                    break;
            }
        }
        if (tile.HasEnemy)
        {
            renderer.material.color = Color.red;
        }
    }
    
    private void BuildWalls(Tile tile, Vector3 position, Transform parent)
    {
        float wallHeight = WALL_HEIGHT;
        Vector3 wallCenter = new Vector3(0, wallHeight / 2f, 0);
        
        // Северная стена
        if (tile.WallNorth)
        {
            GameObject wall = Instantiate(wallPrefab, position + wallCenter + new Vector3(0, 0, CELL_SIZE / 2f), Quaternion.identity, parent);
            wall.transform.localScale = new Vector3(CELL_SIZE, wallHeight, 0.2f);
            ApplyWallMaterial(wall);
        }
        
        // Южная стена
        if (tile.WallSouth)
        {
            GameObject wall = Instantiate(wallPrefab, position + wallCenter + new Vector3(0, 0, -CELL_SIZE / 2f), Quaternion.identity, parent);
            wall.transform.localScale = new Vector3(CELL_SIZE, wallHeight, 0.2f);
            ApplyWallMaterial(wall);
        }
        
        // Восточная стена
        if (tile.WallEast)
        {
            GameObject wall = Instantiate(wallPrefab, position + wallCenter + new Vector3(CELL_SIZE / 2f, 0, 0), Quaternion.identity, parent);
            wall.transform.localScale = new Vector3(0.2f, wallHeight, CELL_SIZE);
            ApplyWallMaterial(wall);
        }
        
        // Западная стена
        if (tile.WallWest)
        {
            GameObject wall = Instantiate(wallPrefab, position + wallCenter + new Vector3(-CELL_SIZE / 2f, 0, 0), Quaternion.identity, parent);
            wall.transform.localScale = new Vector3(0.2f, wallHeight, CELL_SIZE);
            ApplyWallMaterial(wall);
        }
    }
    
    private void ApplyWallMaterial(GameObject wall)
    {
        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (wallMaterial != null) renderer.material = wallMaterial;
            else renderer.material.color = wallColor;
        }
    }
    
    public Vector3 GetCellWorldPosition(Vector2Int cellPosition)
    {
        return new Vector3(cellPosition.x * CELL_SIZE, 0, cellPosition.y * CELL_SIZE);
    }
    
    public Vector2Int GetCellFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / CELL_SIZE);
        int z = Mathf.RoundToInt(worldPosition.z / CELL_SIZE);
        return new Vector2Int(x, z);
    }
    
    public float GetCellSize() => CELL_SIZE;
}