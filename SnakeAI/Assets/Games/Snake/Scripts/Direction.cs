using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.Snake
{
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    public static class DirectionExtension
    {
        public static Direction Right (this Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return Direction.East;
                case Direction.East: return Direction.South;
                case Direction.South: return Direction.West;
                case Direction.West: return Direction.North;
            }

            return Direction.North; // Unreachable
        }
        public static Direction Left (this Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return Direction.West;
                case Direction.East: return Direction.North;
                case Direction.South: return Direction.East;
                case Direction.West: return Direction.South;
            }

            return Direction.North; // Unreachable
        }



        public static Vector2Int ToV2I (this Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return Vector2Int.up;
                case Direction.East: return Vector2Int.right;
                case Direction.South: return Vector2Int.down;
                case Direction.West: return Vector2Int.left;
            }

            return Vector2Int.zero; // Unreachable
        }


        // What is the position AHEAD?
        public static Vector2Int Ahead (this Vector2Int position, Direction direction)
        {
            return position + direction.ToV2I();
        }

        // What is the position to the RIGHT?
        public static Vector2Int Left (this Vector2Int position, Direction direction)
        {
            return position + direction.Left().ToV2I();
        }

        // What is the position to the RIGHT?
        public static Vector2Int Right (this Vector2Int position, Direction direction)
        {
            return position + direction.Right().ToV2I();
        }
    }
}