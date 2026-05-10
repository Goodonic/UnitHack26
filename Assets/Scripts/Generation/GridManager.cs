using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    private Tile[,] grid;
    public int Width { get; private set; }
    public int Height { get; private set; }

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

    public void InitializeGrid(int width, int height)
    {
        Width = width;
        Height = height;
        grid = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Tile(new Vector2Int(x, y));
            }
        }
    }

    public Tile GetTile(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            return grid[x, y];
        return null;
    }

    public Tile GetTile(Vector2Int position)
    {
        return GetTile(position.x, position.y);
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
    }

    public Tile[,] GetGrid() => grid;
}