using UnityEngine;

[System.Serializable]
public class Tile
{
    public Vector2Int Position;
    public bool WallNorth, WallSouth, WallEast, WallWest;
    public TileType GroundType;
    public bool IsStart;
    public bool IsExit;

    public Tile(Vector2Int position)
    {
        Position = position;
        WallNorth = WallSouth = WallEast = WallWest = true;
        GroundType = TileType.Stone;
        IsStart = false;
        IsExit = false;
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