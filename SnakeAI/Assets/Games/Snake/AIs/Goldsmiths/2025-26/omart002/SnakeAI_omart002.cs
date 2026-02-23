using AlanZucconi.AI.BT;
using AlanZucconi.AI.PF;
using AlanZucconi.Snake;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Snake.omart002
{
    [CreateAssetMenu(fileName = "SnakeAI_omart002",
                     menuName = "Snake/2025-26/SnakeAI_omart002")]
    public class SnakeAI_omart002 : SnakeAI
    {
        private WeightedGrid grid;
        public override Node CreateBehaviourTree(SnakeGame Snake)
        {
            return new Selector
               (
                new Filter
                    (
                        () =>
                        {
                            if (grid == null)
                            {
                                grid = new(Snake);
                            }
                            else
                            {
                                grid.UpdateGrid();
                            }

                            //Debug.Log("-----");
                            if (!CanTraverse(Snake, Snake.FoodPosition))
                            {
                                return false;
                            }
                            else
                            {

                                int distance = Snake.DistanceFrom(Snake.HeadPosition, Snake.FoodPosition);
                                if (distance > Snake.Body.Count || distance == 1)
                                    return true;


                                Vector2Int newTailPosition = Snake.Body.ToList()[Snake.Body.Count - distance];
                                var reachableTail = Snake.AvailableNeighbours(newTailPosition).FirstOrDefault(position => Snake.DistanceFrom(Snake.FoodPosition, position) != int.MaxValue);

                                //if (reachableTail != new Vector2Int(0, 0))
                                //Debug.Log("FOOD");
                                return (reachableTail != new Vector2Int(0, 0) || WallNeighbours(Snake, Snake.FoodPosition) == 3);
                            }
                        }, // If food is reachable...
                        new Action(() => Snake.MoveTowards(grid.MoveTowardsFood())) // ...move towards the food
                    ),
               new Action
               (
                    () =>
                    {
                    //Debug.Log("AVOIDING");
                    Vector2Int target = new();
                    var potentialMoves = grid.Nodes[Snake.HeadPosition].ToList();
                    if (Snake.IsFoodReachable())
                    {


                        if (potentialMoves.Count() > 0)
                        {
                            var foodDistance = potentialMoves.Select(pos => Snake.DistanceFrom(pos.Item1, Snake.FoodPosition) * (.5f + 1.5f - pos.Item2)).Where(i => i != int.MaxValue);

                            if (foodDistance.Count() > 0)
                            {
                                //Debug.Log("FOOD");
                                float maxFoodDistance = foodDistance.Max();
                                target = potentialMoves.First(pos => Snake.DistanceFrom(pos.Item1, Snake.FoodPosition) * (.5f + 1.5f - pos.Item2)  == maxFoodDistance).Item1;
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
                            var newTarget = grid.ReachForBody(body);
                            if (newTarget != new Vector2(0,0)) {

                                target = newTarget;
                                break;
                            }


                        }

                            if (target == new Vector2Int(0, 0))
                                Debug.Log("Body Check gone wrong");

                        
                        

                        var targetDistance = potentialMoves.Select(pos => Snake.DistanceFrom(pos.Item1, target) * (.5f + 1.5f - pos.Item2)).Where(i => i != int.MaxValue);
                        if (targetDistance.Count() > 0)
                        {
                            //Debug.Log("TAIL");
                            float maxDistance = targetDistance.Max();
                            target = potentialMoves.First(pos => Snake.DistanceFrom(pos.Item1, target) * (.5f + 1.5f - pos.Item2) == maxDistance).Item1;
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
                            var newTarget = potentialMoves.Where(pos => Snake.DistanceFrom(Snake.TailPosition, pos.Item1) != int.MaxValue);
                            if (newTarget.Count() > 0)
                            {
                                //Debug.Log("Trying to view tail");
                                target = newTarget.First().Item1;
                            }
                            else
                            {
                                var foodDistance = potentialMoves.Select(pos => Snake.DistanceFrom(pos.Item1, Snake.FoodPosition) * (.5f + 1.5f - pos.Item2)).Where(i => i != int.MaxValue);

                                if (foodDistance.Count() > 0)
                                {
                                    //Debug.Log("FOOD");
                                    float maxFoodDistance = foodDistance.Max();
                                    target = potentialMoves.First(pos => Snake.DistanceFrom(pos.Item1, Snake.FoodPosition) * (.5f + 1.5f - pos.Item2) == maxFoodDistance).Item1;
                                }
                            }
                                
                        }


                        if (target == new Vector2Int(0, 0))
                        {
                            target = Snake.AvailableNeighbours(Snake.HeadPosition).FirstOrDefault();
                        }

                        Snake.MoveTowards(target);




                    }
                )
            );
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


        public class WeightedGrid : WeightedGraph<Vector2Int, Edge>
        {
            private SnakeGame snake;

            private static Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            private Dictionary<Vector2Int, bool> walls = new();

            private Vector2 midpoint;


            public WeightedGrid(SnakeGame _snake)
            {
                snake = _snake;
                midpoint = (Vector2)snake.GridSize / 2;

                for (int x = 0; x < snake.GridSize.x; x++)
                {
                    for (int y = 0; y < snake.GridSize.y; y++)
                    {
                        walls.Add(new Vector2Int(x, y), true);
                    }
                }

                UpdateGrid();
            }

            public void UpdateGrid()
            {
                foreach (Vector2Int v in walls.Keys.ToList())
                {
                    SnakeGame.Cell cell = snake[v];
                    bool isWall = (cell == SnakeGame.Cell.None) || (cell == SnakeGame.Cell.Snake && v != snake.HeadPosition);
                    if (isWall != walls[v])
                    {
                        walls[v] = isWall;
                        SetWall(v);
                    }                
                }
                var deadEnds = (walls.Keys.Where(w => w != snake.HeadPosition && Nodes.ContainsKey(w) && Nodes[w].Count() == 1 ));
                while (deadEnds.Count() > 0)
                {
                    foreach (Vector2Int v in deadEnds.ToList())
                    {
                        walls[v] = true;
                        SetWall(v); 
                    }
                }

            }

            public void SetWall(Vector2Int v)
            {
                foreach (Vector2Int v2 in directions)
                {
                    Vector2Int newLocation = v + v2;

                    if (walls.ContainsKey(newLocation))
                    {
                        if (walls[v])
                        {
                            Disconnect(v, newLocation);
                            Disconnect(newLocation, v);
                        }
                        else
                        {
                            if (!walls[newLocation])
                            {
                                float currentDistance = Mathf.Max(Mathf.Abs(v.x-midpoint.x)/snake.GridSize.x, Mathf.Abs(v.y-midpoint.y)/snake.GridSize.y);
                                float newDistance = Mathf.Max(Mathf.Abs(newLocation.x - midpoint.x) / snake.GridSize.x, Mathf.Abs(newLocation.y - midpoint.y) / snake.GridSize.y);

                                float toWeight = 1;
                                float fromWeight = 1;

                                if (newDistance > currentDistance)
                                {
                                    toWeight = 1.5f;
                                    fromWeight = 0.75f;
                                }
                                else if (newDistance < currentDistance)
                                {
                                    fromWeight = 1.5f;
                                    toWeight = 0.75f;
                                }
         


                                Connect(v, newLocation, toWeight);
                                Connect(newLocation, v, fromWeight);
                            }
                        }
                    }
                }
            }

            public Vector2Int MoveTowardsFood()
            {


                return Pathfind(snake.HeadPosition, snake.FoodPosition);
            }


            public Vector2Int Pathfind(Vector2Int start,  Vector2Int end)
            {
                var path = this.AStar(
                    start, end,
                    (a, b) => Mathf.Abs(b.x - a.x) + Mathf.Abs(b.y - a.y));
                if (path != null && path.Count() > 1)
                {
                    return path[1].Item1;
                }
                else
                {
                    return new Vector2Int(0,0);
                }
            }

            public Vector2Int ReachForBody(Vector2Int body)
            {
                List<Vector2Int> neighbours = Nodes[body].Select(n => n.Item1).ToList();
                return neighbours.Select(n => Pathfind(snake.HeadPosition, n)).Where(path => path != new Vector2Int(0, 0)).FirstOrDefault();
            }
        }
    }



}
