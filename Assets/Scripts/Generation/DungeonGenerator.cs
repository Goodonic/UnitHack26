using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UI;

namespace Generation
{
    public class DungeonGenerator : MonoBehaviour
    {
        public static DungeonGenerator Instance { get; private set; }

        [Header("Generation Settings")]
        [SerializeField] private int width = 41;
        [SerializeField] private int height = 41;
        [SerializeField] private int enemyCount = 5;
        [SerializeField] private Vector2Int startPosition = new Vector2Int(1, 1);

        [Header("Floors")]
        [SerializeField] private int maxFloors = 3;

        public int CurrentFloor { get; private set; } = 1;
        public int MaxFloors => maxFloors;

        [Header("Enemies")]
        [SerializeField] private List<EnemyData> possibleEnemies;
        
        [Header("Boss")]
        [SerializeField] private EnemyData bossEnemy;
        
        public EnemyData BossEnemy => bossEnemy;
        
        [Header("BSP Settings")]
        [SerializeField] private int minLeafSize = 8;
        [SerializeField] private int maxLeafSize = 16;
        [SerializeField] private int minRoomSize = 4;
        [SerializeField] private int roomPadding = 1;

        [Header("Cave Rooms")]
        [SerializeField, Range(0f, 1f)] private float caveRoomChance = 0.35f;
        [SerializeField, Range(0f, 1f)] private float caveTileRemoveChance = 0.22f;
        
        [Header("Ground")]
        [SerializeField, Range(0f, 1f)] private float dirtChance = 0.18f;
        [SerializeField, Range(0f, 1f)] private float waterChance = 0.05f;


        private GridManager gridManager;
        private readonly List<BspLeaf> leaves = new List<BspLeaf>();
        private readonly List<Room> rooms = new List<Room>();
        private readonly HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

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

        private void Start()
        {
            gridManager = GridManager.Instance;
            GenerateDungeon();
        }

        public void GenerateDungeon()
        {
            gridManager.InitializeGrid(width, height);

            leaves.Clear();
            rooms.Clear();
            floorPositions.Clear();

            BspLeaf root = new BspLeaf(new RectInt(1, 1, width - 2, height - 2));
            SplitLeaf(root);
            CollectLeaves(root);

            CreateRooms();
            ConnectRooms();
            
            ApplyWalls();
            PlaceStartAndExit();

            PlaceEnemies();
            MinimapRenderer.Instance?.DrawMap(gridManager.GetGrid());
            LevelBuilder.Instance.BuildLevel(gridManager.GetGrid());
        }

        private void PlaceEnemies()
        {
            if (possibleEnemies == null || possibleEnemies.Count == 0)
            {
                Debug.LogWarning("DangeonGenerator: Список possibleEnemies пуст! Враги не будут созданы.");
                return;
            }

            List<Tile> candidates = new List<Tile>();

            foreach (Vector2Int position in floorPositions)
            {
                Tile tile = gridManager.GetTile(position);

                if (tile == null) continue;
                if (tile.IsStart || tile.IsExit) continue;
                if (tile.HasEnemy) continue;

                candidates.Add(tile);
            }

            for (int i = 0; i < enemyCount; i++)
            {
                if (candidates.Count == 0) return;

                int index = Random.Range(0, candidates.Count);
                Tile selected = candidates[index];

                // ВЫБИРАЕМ СЛУЧАЙНОГО ВРАГА ИЗ СПИСКА И КЛАДЕМ В ТАЙЛ
                EnemyData randomEnemy = possibleEnemies[Random.Range(0, possibleEnemies.Count)];
                selected.EnemyDataOnTile = randomEnemy;

                candidates.RemoveAt(index);
            }
        }

        private void GenerateMazeRecursiveBacktracker()
        {
            AssignGroundTypes();

            LevelBuilder.Instance.BuildLevel(gridManager.GetGrid());

            if (MinimapRenderer.Instance != null)
            {
                MinimapRenderer.Instance.DrawMap(gridManager.GetGrid());
            }

            Debug.Log($"Generated floor {CurrentFloor}/{maxFloors}");
        }

        public bool TryGoToNextFloor()
        {
            if (CurrentFloor >= maxFloors)
            {
                Debug.Log("Dungeon complete! All floors finished.");
                return false;
            }

            CurrentFloor++;
            GenerateDungeon();
            return true;
        }
           
        private void SplitLeaf(BspLeaf leaf)
        {
            if (leaf.Area.width <= maxLeafSize && leaf.Area.height <= maxLeafSize)
            {
                return;
            }

            bool splitHorizontally;

            if (leaf.Area.width > leaf.Area.height && leaf.Area.width / (float)leaf.Area.height >= 1.25f)
            {
                splitHorizontally = false;
            }
            else if (leaf.Area.height > leaf.Area.width && leaf.Area.height / (float)leaf.Area.width >= 1.25f)
            {
                splitHorizontally = true;
            }
            else
            {
                splitHorizontally = Random.value > 0.5f;
            }

            int max = splitHorizontally ? leaf.Area.height - minLeafSize : leaf.Area.width - minLeafSize;

            if (max <= minLeafSize)
            {
                return;
            }

            int split = Random.Range(minLeafSize, max + 1);

            if (splitHorizontally)
            {
                leaf.Left = new BspLeaf(new RectInt(
                    leaf.Area.x,
                    leaf.Area.y,
                    leaf.Area.width,
                    split));

                leaf.Right = new BspLeaf(new RectInt(
                    leaf.Area.x,
                    leaf.Area.y + split,
                    leaf.Area.width,
                    leaf.Area.height - split));
            }
            else
            {
                leaf.Left = new BspLeaf(new RectInt(
                    leaf.Area.x,
                    leaf.Area.y,
                    split,
                    leaf.Area.height));

                leaf.Right = new BspLeaf(new RectInt(
                    leaf.Area.x + split,
                    leaf.Area.y,
                    leaf.Area.width - split,
                    leaf.Area.height));
            }

            SplitLeaf(leaf.Left);
            SplitLeaf(leaf.Right);
        }

        private void CollectLeaves(BspLeaf leaf)
        {
            if (leaf == null)
            {
                return;
            }

            if (leaf.IsLeaf)
            {
                leaves.Add(leaf);
                return;
            }

            CollectLeaves(leaf.Left);
            CollectLeaves(leaf.Right);
        }

        private void CreateRooms()
        {
            foreach (BspLeaf leaf in leaves)
            {
                int availableWidth = leaf.Area.width - roomPadding * 2;
                int availableHeight = leaf.Area.height - roomPadding * 2;

                if (availableWidth < minRoomSize || availableHeight < minRoomSize)
                {
                    continue;
                }

                int roomWidth = Random.Range(minRoomSize, availableWidth + 1);
                int roomHeight = Random.Range(minRoomSize, availableHeight + 1);

                int roomX = Random.Range(
                    leaf.Area.x + roomPadding,
                    leaf.Area.xMax - roomPadding - roomWidth + 1);

                int roomY = Random.Range(
                    leaf.Area.y + roomPadding,
                    leaf.Area.yMax - roomPadding - roomHeight + 1);

                Room room = new Room(new RectInt(roomX, roomY, roomWidth, roomHeight));
                rooms.Add(room);
                leaf.Room = room;

                if (Random.value < caveRoomChance)
                {
                    CarveCaveRoom(room);
                }
                else
                {
                    CarveRectRoom(room);
                }
            }
        }

        private void CarveRectRoom(Room room)
        {
            for (int x = room.Area.xMin; x < room.Area.xMax; x++)
            {
                for (int y = room.Area.yMin; y < room.Area.yMax; y++)
                {
                    CarveFloor(new Vector2Int(x, y));
                }
            }
        }

        private void CarveCaveRoom(Room room)
        {
            Vector2 center = room.Center;
            float radiusX = Mathf.Max(1f, room.Area.width * 0.5f);
            float radiusY = Mathf.Max(1f, room.Area.height * 0.5f);

            for (int x = room.Area.xMin; x < room.Area.xMax; x++)
            {
                for (int y = room.Area.yMin; y < room.Area.yMax; y++)
                {
                    Vector2 position = new Vector2(x + 0.5f, y + 0.5f);
                    float normalizedDistance =
                        Mathf.Pow((position.x - center.x) / radiusX, 2f) +
                        Mathf.Pow((position.y - center.y) / radiusY, 2f);

                    bool insideCaveShape = normalizedDistance <= Random.Range(0.75f, 1.25f);
                    bool keepTile = Random.value > caveTileRemoveChance;

                    if (insideCaveShape && keepTile)
                    {
                        CarveFloor(new Vector2Int(x, y));
                    }
                }
            }

            CarveFloor(room.CenterCell);
        }

        private void ConnectRooms()
        {
            if (rooms.Count <= 1)
            {
                return;
            }

            List<Room> orderedRooms = rooms
                .OrderBy(room => room.CenterCell.x)
                .ThenBy(room => room.CenterCell.y)
                .ToList();

            for (int i = 0; i < orderedRooms.Count - 1; i++)
            {
                Vector2Int from = orderedRooms[i].CenterCell;
                Vector2Int to = orderedRooms[i + 1].CenterCell;

                CarveCorridor(from, to);
            }

            for (int i = 0; i < orderedRooms.Count / 3; i++)
            {
                Room a = orderedRooms[Random.Range(0, orderedRooms.Count)];
                Room b = orderedRooms[Random.Range(0, orderedRooms.Count)];

                if (a != b)
                {
                    CarveCorridor(a.CenterCell, b.CenterCell);
                }
            }
        }

        private void CarveCorridor(Vector2Int from, Vector2Int to)
        {
            Vector2Int current = from;
            CarveFloor(current);

            bool horizontalFirst = Random.value > 0.5f;

            if (horizontalFirst)
            {
                while (current.x != to.x)
                {
                    current.x += current.x < to.x ? 1 : -1;
                    CarveFloor(current);
                }

                while (current.y != to.y)
                {
                    current.y += current.y < to.y ? 1 : -1;
                    CarveFloor(current);
                }
            }
            else
            {
                while (current.y != to.y)
                {
                    current.y += current.y < to.y ? 1 : -1;
                    CarveFloor(current);
                }

                while (current.x != to.x)
                {
                    current.x += current.x < to.x ? 1 : -1;
                    CarveFloor(current);
                }
            }
        }

        private void CarveFloor(Vector2Int position)
        {
            if (!gridManager.IsValidPosition(position))
            {
                return;
            }

            floorPositions.Add(position);
        }

        private void ApplyWalls()
        {
            foreach (Vector2Int position in floorPositions)
            {
                Tile tile = gridManager.GetTile(position);

                tile.WallNorth = !floorPositions.Contains(position + Vector2Int.up);
                tile.WallSouth = !floorPositions.Contains(position + Vector2Int.down);
                tile.WallEast = !floorPositions.Contains(position + Vector2Int.right);
                tile.WallWest = !floorPositions.Contains(position + Vector2Int.left);
            }
        }

        private void PlaceStartAndExit()
        {
            if (rooms.Count == 0)
            {
                return;
            }

            Room startRoom = rooms[Random.Range(0, rooms.Count)];
            Vector2Int startPosition = FindNearestFloor(startRoom.CenterCell);

            Room exitRoom = rooms
                .OrderByDescending(room => Vector2Int.Distance(startPosition, room.CenterCell))
                .First();

            Vector2Int exitPosition = FindNearestFloor(exitRoom.CenterCell);

            if (exitPosition == startPosition && floorPositions.Count > 1)
            {
                exitPosition = floorPositions
                    .OrderByDescending(position => Vector2Int.Distance(startPosition, position))
                    .First();
            }

            Tile startTile = gridManager.GetTile(startPosition);
            Tile exitTile = gridManager.GetTile(exitPosition);

            startTile.IsStart = true;
            startTile.GroundType = TileType.Start;

            exitTile.IsExit = true;
            exitTile.GroundType = TileType.Exit;
        }

        private Vector2Int FindNearestFloor(Vector2Int preferredPosition)
        {
            if (floorPositions.Contains(preferredPosition))
            {
                return preferredPosition;
            }

            return floorPositions
                .OrderBy(position => Vector2Int.Distance(preferredPosition, position))
                .First();
        }

        private void AssignGroundTypes()
        {
            foreach (Vector2Int position in floorPositions)
            {
                Tile tile = gridManager.GetTile(position);

                if (tile.IsStart)
                {
                    tile.GroundType = TileType.Start;
                    continue;
                }
                
                if (tile.IsExit)
                {
                    tile.GroundType = TileType.Exit;
                    continue;
                }
                
                float random = Random.value;

                if (random < waterChance)
                {
                    tile.GroundType = TileType.Water;
                }
                else if (random < waterChance + dirtChance)
                {
                    tile.GroundType = TileType.Dirt;
                }
                else
                {
                    tile.GroundType = TileType.Stone;
                }
            }
        }

        private sealed class BspLeaf
        {
            public readonly RectInt Area;
            public BspLeaf Left;
            public BspLeaf Right;
            public Room Room;

            public bool IsLeaf => Left == null && Right == null;

            public BspLeaf(RectInt area)
            {
                Area = area;
            }
        }

        private sealed class Room
        {
            public readonly RectInt Area;

            public Vector2 Center => new Vector2(
                Area.x + Area.width * 0.5f,
                Area.y + Area.height * 0.5f);

            public Vector2Int CenterCell => new Vector2Int(
                Area.x + Area.width / 2,
                Area.y + Area.height / 2);

            public Room(RectInt area)
            {
                Area = area;
            }
        }
    }
}