using UnityEngine;

[System.Serializable]
public class Tile
{
    public Vector2Int Position;
    public bool WallNorth, WallSouth, WallEast, WallWest;
    public TileType GroundType;

    public bool HasChest { get; set; }
    public Chest ChestOnTile { get; set; }

    public bool IsStart;
    public bool IsExit;

    public EnemyData EnemyDataOnTile;

    public bool HasEnemy => EnemyDataOnTile != null;

    public Tile(Vector2Int position)
    {
        Position = position;
        WallNorth = WallSouth = WallEast = WallWest = true;
        GroundType = TileType.Stone;
        IsStart = false;
        IsExit = false;
        EnemyDataOnTile = null;
    }

    public bool HasWall(Direction direction)
    {
        return direction switch
        {
            Direction.North => WallNorth,
            Direction.South => WallSouth,
            Direction.East => WallEast,
            Direction.West => WallWest,
            _ => false
        };
    }

    public void SetWall(Direction direction, bool value)
    {
        switch (direction)
        {
            case Direction.North: WallNorth = value; break;
            case Direction.South: WallSouth = value; break;
            case Direction.East: WallEast = value; break;
            case Direction.West: WallWest = value; break;
        }
    }
}