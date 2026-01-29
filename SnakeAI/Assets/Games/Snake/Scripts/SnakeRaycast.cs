using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.Snake
{

    public struct SnakecastHit
    {
        public Vector2Int Position;
        public int Distance;
        public SnakeGame.Cell Cell;
    }

    // Extension to the SnakeGame for raycast
    public static class SnakeRaycastExtension
    {
        

        //public static SnakecastHit Raycast(this SnakeGame snake, Vector2Int position, Vector2Int direction)
        public static SnakecastHit Raycast(this SnakeGame snake, Vector2Int position, Direction direction)
        {
            int distance = 0;

            position += direction.ToV2I();

        //while (snake.IsFree(position))
        while (snake.IsEmpty(position))
        {
                distance++;
                position += direction.ToV2I();
            }
            

            return new SnakecastHit
            {
                Position = position,
                Distance = distance,
                Cell = snake[position]
            };
        }

        // Raycast from the snake head
        public static SnakecastHit Raycast(this SnakeGame snake, Direction direction)
        {
            return Raycast(snake, snake.HeadPosition, direction);
        }

        #region Global
        public static SnakecastHit RaycastNorth (this SnakeGame snake)
        {
            return Raycast(snake, snake.HeadPosition, Direction.North);
        }
        public static SnakecastHit RaycastSouth(this SnakeGame snake)
        {
            return Raycast(snake, snake.HeadPosition, Direction.South);
        }
        public static SnakecastHit RaycastEast(this SnakeGame snake)
        {
            return Raycast(snake, snake.HeadPosition, Direction.East);
        }
        public static SnakecastHit RaycastWest(this SnakeGame snake)
        {
            return Raycast(snake, snake.HeadPosition, Direction.West);
        }
        #endregion

        #region Local
        public static SnakecastHit RaycastAhead(this SnakeGame snake)
        {
            return Raycast(snake, snake.HeadPosition, snake.Direction);
        }
        public static SnakecastHit RaycastLeft(this SnakeGame snake)
        {
            return Raycast(snake, snake.HeadPosition, snake.Direction.Left());
        }
        public static SnakecastHit RaycastRight(this SnakeGame snake)
        {
            return Raycast(snake, snake.HeadPosition, snake.Direction.Right());
        }
        #endregion

    }
}