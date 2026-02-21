using AlanZucconi.AI.BT;
using AlanZucconi.AI.PF;
using AlanZucconi.Snake;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Snake.omart002
{
    [CreateAssetMenu(fileName = "SnakeAI_omart002",
                     menuName = "Snake/2025-26/SnakeAI_omart002")]
    public class SnakeAI_omart002 : SnakeAI
    {
        public override Node CreateBehaviourTree(SnakeGame Snake)
        {
            return new Selector
               (
                new Filter
                    (
                        () =>
                        {
                            //Debug.Log("-----");
                            if (!CanTraverse(Snake, Snake.FoodPosition))
                            {
                                return false;
                            }
                            else
                            {

                                int distance = Snake.DistanceFrom(Snake.HeadPosition, Snake.FoodPosition);
                                if (distance > Snake.Body.Count || Snake.Body.Count == 0 || distance == 1)
                                    return true;


                                Vector2Int newTailPosition = Snake.Body.ToList()[Snake.Body.Count - distance];
                                var reachableTail = Snake.AvailableNeighbours(newTailPosition).FirstOrDefault(position => Snake.DistanceFrom(Snake.FoodPosition, position) != int.MaxValue);

                                //if (reachableTail != new Vector2Int(0, 0))
                                //Debug.Log("FOOD");
                                return (reachableTail != new Vector2Int(0, 0) || WallNeighbours(Snake, Snake.FoodPosition) == 3);
                            }
                        }, // If food is reachable...
                        new Action(Snake.MoveTowardsFood) // ...move towards the food
                    ),
               new Action
               (
                    () =>
                    {
                    //Debug.Log("AVOIDING");
                    Vector2Int target = new();
                    var potentialMoves = Snake.AvailableNeighbours(Snake.HeadPosition).Where(pos => CanTraverse(Snake, pos));
                    if (Snake.IsFoodReachable() && false)
                    {


                        if (potentialMoves.Count() > 0)
                        {
                            var foodDistance = potentialMoves.Select(pos => Snake.DistanceFrom(pos, Snake.FoodPosition)).Where(i => i != int.MaxValue);

                            if (foodDistance.Count() > 0)
                            {
                                //Debug.Log("FOOD");
                                int maxFoodDistance = foodDistance.Max();
                                target = potentialMoves.First(pos => Snake.DistanceFrom(pos, Snake.FoodPosition) == maxFoodDistance);
                            }
                        }

                    }
                    else
                    {

                        List<Vector2Int> reversedBody = Snake.Body.Reverse().ToList();
                        int idx = 0;

                        foreach (Vector2Int body in reversedBody)
                        {
                            idx++;
                            var newTarget = Snake.AvailableNeighbours(body).Where(position => CanTraverse(Snake, position));
                            if (newTarget.Count() > 0)
                            {
                                SnakePathfinding pf = new SnakePathfinding(Snake);
                                target = pf.BreadthFirstSearch(Snake.HeadPosition, newTarget.First())[1]    ;
                                break;
                            }


                        }

                        var targetDistance = potentialMoves.Select(pos => Snake.DistanceFrom(pos, target)).Where(i => i != int.MaxValue);
                        if (targetDistance.Count() > 0)
                        {
                            //Debug.Log("TAIL");
                            int maxDistance = targetDistance.Max();
                            target = potentialMoves.First(pos => Snake.DistanceFrom(pos, target) == maxDistance);
                        }

                            /*
                            if (Snake.DistanceFrom(Snake.HeadPosition, target) < idx && !Snake.IsFoodReachable())
                            {
                                Debug.Log("----------");
                                Debug.Log(target);
                                target = GetFurthest(Snake, target);
                                Debug.Log(target);
                            }*/
                        }

                        if (Snake.DistanceFrom(Snake.TailPosition, target) == int.MaxValue)
                        {
                            var newTarget = potentialMoves.Where(pos => Snake.DistanceFrom(Snake.TailPosition, pos) != int.MaxValue);
                            if (newTarget.Count() > 0)
                            {
                                //Debug.Log("Trying to view tail");
                                target = newTarget.First();
                            }
                            else
                            {
                                var foodDistance = potentialMoves.Select(pos => Snake.DistanceFrom(pos, Snake.FoodPosition)).Where(i => i != int.MaxValue);

                                if (foodDistance.Count() > 0)
                                {
                                    //Debug.Log("FOOD");
                                    int maxFoodDistance = foodDistance.Max();
                                    target = potentialMoves.First(pos => Snake.DistanceFrom(pos, Snake.FoodPosition) == maxFoodDistance);
                                }
                            }
                                
                        }

                        Snake.MoveTowards(target);




                    }
                )
            );
        }

        private Vector2Int GetFurthest(SnakeGame Snake, Vector2Int target)
        {
            Vector2Int furthest = new();
            int maxDistance = 0;
            for (int x = 0; x < Snake.GridSize.x; x++)
            {
                for (int y = 0; y < Snake.GridSize.y; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);

                    if (CanTraverse(Snake, coord))
                    {
                        if (Snake.DistanceFrom(target, coord) > maxDistance)
                        {
                            maxDistance = Snake.DistanceFrom(target, coord);
                            furthest = coord;

                            if (maxDistance > Snake.Body.Count)
                            {
                                return furthest;
                            }
                        }
                    }
                }
            }
            return furthest;
        }


        private bool CanTraverse(SnakeGame snake, Vector2Int location)
        {
            var neighbours = snake.AvailableNeighbours(location);
            int emptyNeighbours = neighbours.Count();
            int wallNeighbours = WallNeighbours(snake, location);

            return (location != snake.HeadPosition) && (snake.IsReachable(location) && (emptyNeighbours > 1 || (snake.DistanceFrom(snake.HeadPosition, location) == 1 && emptyNeighbours > 0) || (snake.IsFood(location) && wallNeighbours > 2)));

        }

        private int WallNeighbours(SnakeGame snake, Vector2Int location)
        {
            int walls = 0;

            for (int idx = 0; idx <= 1; idx++)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2Int newCoord = location;
                    newCoord[idx] += i;
                    if (snake.IsWall(newCoord))
                        walls++;
                }

            }
            return walls;
        }
    }



}
