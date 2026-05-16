using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public class MinimapRenderer : MonoBehaviour
    {
        public static MinimapRenderer Instance { get; private set; }
        
        [Header("UI")]
        [SerializeField] private RawImage minimapImage;
        
        [Header("Colors")]
        [SerializeField] private Color wallColor = new Color(0.02f, 0.02f, 0.02f, 1f);
        [SerializeField] private Color stoneColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        [SerializeField] private Color dirtColor = new Color(0.45f, 0.28f, 0.12f, 1f);
        [SerializeField] private Color waterColor = new Color(0.05f, 0.2f, 0.8f, 1f);
        [SerializeField] private Color startColor = Color.yellow;
        [SerializeField] private Color exitColor = Color.green;
        [SerializeField] private Color playerColor = Color.red;
        
        [Header("Settings")]
        [SerializeField] private FilterMode filterMode = FilterMode.Point;
        
         private Texture2D minimapTexture;
    private Tile[,] currentGrid;
    private Vector2Int playerPosition;
    private bool hasPlayerPosition;

    private void Awake()
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

    public void DrawMap(Tile[,] grid)
    {
        currentGrid = grid;
        hasPlayerPosition = false;

        if (currentGrid == null)
        {
            return;
        }

        int width = currentGrid.GetLength(0);
        int height = currentGrid.GetLength(1);

        minimapTexture = new Texture2D(width, height);
        minimapTexture.filterMode = filterMode;

        if (minimapImage != null)
        {
            minimapImage.texture = minimapTexture;
        }

        Redraw();
    }

    public void SetPlayerPosition(Vector2Int position)
    {
        playerPosition = position;
        hasPlayerPosition = true;

        Redraw();
    }

    private void Redraw()
    {
        if (currentGrid == null || minimapTexture == null)
        {
            return;
        }

        int width = currentGrid.GetLength(0);
        int height = currentGrid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = currentGrid[x, y];
                Color color = GetTileColor(tile);

                minimapTexture.SetPixel(x, y, color);
            }
        }

        if (hasPlayerPosition && IsInsideMap(playerPosition, width, height))
        {
            minimapTexture.SetPixel(playerPosition.x, playerPosition.y, playerColor);
        }

        minimapTexture.Apply();
    }

    private Color GetTileColor(Tile tile)
    {
        bool isClosedCell =
            tile.WallNorth &&
            tile.WallSouth &&
            tile.WallEast &&
            tile.WallWest &&
            !tile.IsStart &&
            !tile.IsExit;

        if (isClosedCell)
        {
            return wallColor;
        }

        if (tile.IsStart)
        {
            return startColor;
        }

        if (tile.IsExit)
        {
            return exitColor;
        }

        return tile.GroundType switch
        {
            TileType.Stone => stoneColor,
            TileType.Dirt => dirtColor,
            TileType.Water => waterColor,
            TileType.Start => startColor,
            TileType.Exit => exitColor,
            _ => stoneColor
        };
    }

    private bool IsInsideMap(Vector2Int position, int width, int height)
    {
        return position.x >= 0 &&
               position.x < width &&
               position.y >= 0 &&
               position.y < height;
    }
    }
}