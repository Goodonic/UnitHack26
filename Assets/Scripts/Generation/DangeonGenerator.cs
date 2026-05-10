using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Generation
{
    public class DangeonGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int height = 21;
        [SerializeField] private int width = 21;
        [SerializeField] private Vector2Int startPosition = new Vector2Int(1, 1);
        
        private GridManager gridManager;
        private Stack<Vector2Int> visitedStack = new Stack<Vector2Int>();
        private HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        void Start()
        {
            gridManager = GridManager.Instance;
            GenerateDungeon();
        }
        
        public void GenerateDungeon()
        {
            gridManager.InitializeGrid(width, height);
            GenerateMazeRecursiveBacktracker();
            PlaceStartAndExit();
            LevelBuilder.Instance.BuildLevel(gridManager.GetGrid());
        }
        
        private void GenerateMazeRecursiveBacktracker()
        {
            Vector2Int start = startPosition;
            visitedStack.Push(start);
            visited.Add(start);

            while (visitedStack.Count > 0)
            {
                Vector2Int current = visitedStack.Peek();
                List<Vector2Int> unvisitedNeighbors = GetUnvisitedNeighbors(current);

                if (unvisitedNeighbors.Count > 0)
                {
                    Vector2Int next = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                    RemoveWallBetween(current, next);
                    visitedStack.Push(next);
                    visited.Add(next);
                }
                else
                {
                    visitedStack.Pop();
                }
            }
        }
        
        private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
        
            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                Vector2Int neighborPos = cell + dir.ToVector();
            
                if (gridManager.IsValidPosition(neighborPos) && !visited.Contains(neighborPos))
                {
                    neighbors.Add(neighborPos);
                }
            }
        
            return neighbors;
        }
        
        private void RemoveWallBetween(Vector2Int a, Vector2Int b)
        {
            Tile tileA = gridManager.GetTile(a);
            Tile tileB = gridManager.GetTile(b);
        
            Vector2Int delta = b - a;
        
            if (delta == new Vector2Int(0, 1)) // B is North of A
            {
                tileA.WallNorth = false;
                tileB.WallSouth = false;
            }
            else if (delta == new Vector2Int(0, -1)) // B is South of A
            {
                tileA.WallSouth = false;
                tileB.WallNorth = false;
            }
            else if (delta == new Vector2Int(1, 0)) // B is East of A
            {
                tileA.WallEast = false;
                tileB.WallWest = false;
            }
            else if (delta == new Vector2Int(-1, 0)) // B is West of A
            {
                tileA.WallWest = false;
                tileB.WallEast = false;
            }
        }
        
        private void PlaceStartAndExit()
        {
            List<Tile> tiles = new List<Tile>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles.Add(gridManager.GetTile(x, y));
                }
            }
            
            List<Tile> validTiles = tiles.Where(t => 
                !t.WallNorth || !t.WallSouth || !t.WallEast || !t.WallWest
            ).ToList();

            if (validTiles.Count >= 2)
            {
                Tile start = validTiles[Random.Range(0, validTiles.Count)];
                Tile exit = validTiles[Random.Range(0, validTiles.Count)];
            
                int attempts = 0;
                while (exit == start && attempts < 100)
                {
                    exit = validTiles[Random.Range(0, validTiles.Count)];
                    attempts++;
                }
            
                start.IsStart = true;
                exit.IsExit = true;
                
                start.GroundType = TileType.Stone;
                exit.GroundType = TileType.Stone;
                
                foreach (var tile in tiles)
                {
                    if (!tile.IsStart && !tile.IsExit)
                    {
                        float rand = Random.value;
                        if (rand < 0.7f) tile.GroundType = TileType.Stone;
                        else if (rand < 0.9f) tile.GroundType = TileType.Dirt;
                        else tile.GroundType = TileType.Water;
                    }
                }
            }
        }
    }
}