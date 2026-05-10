using UnityEngine;

public enum Direction
{
    North,
    South,
    East,
    West
}

public static class DirectionExtensions
{
    public static Vector2Int ToVector(this Direction direction)
    {
        return direction switch
        {
            Direction.North => new Vector2Int(0, 1),
            Direction.South => new Vector2Int(0, -1),
            Direction.East => new Vector2Int(1, 0),
            Direction.West => new Vector2Int(-1, 0),
            _ => Vector2Int.zero
        };
    }
}