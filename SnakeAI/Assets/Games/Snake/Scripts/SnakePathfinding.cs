using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AlanZucconi.AI.PF;

namespace AlanZucconi.Snake
{
    public static class SnakePathfindingExtension
    {
        public static bool IsReachable(this SnakeGame snake, Vector2Int start, Vector2Int end)
        {
            SnakePathfinding pf = new SnakePathfinding(snake);
            List<Vector2Int> path = pf.BreadthFirstSearch(start, end);
            return path != null; // Path is null -> no path
        }

        public static bool IsReachable(this SnakeGame snake, Vector2Int end)
        {
            SnakePathfinding pf = new SnakePathfinding(snake);
            List<Vector2Int> path = pf.BreadthFirstSearch(snake.HeadPosition, end);
            return path != null; // Path is null -> no path
        }

        public static bool IsFoodReachable(this SnakeGame snake)
        {
            return snake.IsReachable(snake.HeadPosition, snake.FoodPosition);
        }


        // Moves the snake towards the target, using pathfinding
        // Returns false if there is no path
        public static void MoveTowards(this SnakeGame snake, Vector2Int end)
        //public static bool MoveTowards(this SnakeGame snake, Vector2Int end)
        {
            SnakePathfinding pf = new SnakePathfinding(snake);
            List<Vector2Int> path = pf.BreadthFirstSearch(snake.HeadPosition, end);
            if (path == null)
                return;
            //return false;

            // Already on the target
            if (path.Count == 1)
                return;

            Vector2Int direction = path[1] - path[0];
            if (direction.x > 0)
                snake.GoEast();
            if (direction.x < 0)
                snake.GoWest();
            if (direction.y > 0)
                snake.GoNorth();
            if (direction.y < 0)
                snake.GoSouth();

            return;
            //return true;
        }
        public static void MoveTowards(this SnakeGame snake, int x, int y)
        {
            MoveTowards(snake, new Vector2Int(x, y));
        }


        //public static bool MoveTowardsFood(this SnakeGame snake)
        public static void MoveTowardsFood(this SnakeGame snake)
        {
            //return snake.MoveTowards(snake.FoodPosition);
            snake.MoveTowards(snake.FoodPosition);
        }

        // Distance between two points
        // returns int.MaxValue is target is unreachable
        // returns 0 is start == end
        public static int DistanceFrom (this SnakeGame snake, Vector2Int start, Vector2Int end)
        {
            SnakePathfinding pf = new SnakePathfinding(snake);
            List<Vector2Int> path = pf.BreadthFirstSearch(start, end);
            if (path == null)
                return int.MaxValue;

            return path.Count - 1;
        }
    }

    // Helper class
    public struct SnakePathfinding : IPathfinding<Vector2Int>
    {
        public SnakeGame Snake;
        public SnakePathfinding (SnakeGame snake)
        {
            Snake = snake;
        }

        // Given a position on the board, which neighbouring cells
        // can the snake move to?
        public IEnumerable<Vector2Int> Outgoing (Vector2Int position)
        {
            return Snake.AvailableNeighbours(position);
            //yield return Snake.Outstar(position);
            //foreach (Vector2Int neighbour in Snake.Outstar(position))
            //    yield return neighbour;
        }

        public Direction? DirectionTowardsFood ()
        //public void DirectionTowardsFood()
        {
            List<Vector2Int> path = this.BreadthFirstSearch(Snake.HeadPosition, Snake.FoodPosition);

            // No path!
            if (path == null)
                return null;

            //Debug.Log(Snake.HeadPosition + "\t" + Snake.FoodPosition);
            //foreach (var x in path)
            //    Debug.Log("\t" + x);

            path.Add(Snake.FoodPosition);

            // Direction
            Vector2Int direction = path[1] - path[0];
            if (direction.x > 0)
                return Direction.East;
            if (direction.x < 0)
                return Direction.West;
            if (direction.y > 0)
                return Direction.North;
            if (direction.y < 0)
                return Direction.South;

            // unreachable
            return null;
        }
    }
}