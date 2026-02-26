using AlanZucconi.AI.BT;
using AlanZucconi.AI.PF;
using AlanZucconi.Snake;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;


namespace Snake.omart002_v2
{
    [CreateAssetMenu(fileName = "SnakeAI_omart002_v2",
                     menuName = "Snake/2025-26/SnakeAI_omart002_v2")]
    public class SnakeAI_omart002_v2 : SnakeAI
    {
        private WeightedGrid grid;
        private Queue<Vector2Int> avoidPath = new();
        public override Node CreateBehaviourTree(SnakeGame snake)
        {
            return new Action(() =>
            {
                if (grid == null)
                {
                    grid = new(snake);
                }
                else
                {
                    grid.UpdateGrid();
                }

                Vector2Int target;
                Vector2Int? foodPath = grid.FoodPath();
                if (foodPath.HasValue)
                {
                    if (!avoidPath.IsEmpty())
                        avoidPath.Clear();

                    Debug.Log(foodPath.Value);
                    target = foodPath.Value;
                }
                else
                {



                    //Debug.Log("Random");
                    // Debug.Log("TRYING AVOUD");
                    Debug.Log(avoidPath.Count());
                    if (avoidPath.IsEmpty())
                    {
                       
                        var path = grid.AvoidUntilReachable(snake.FoodPosition);

                        if (path != null)
                        {
                            avoidPath = new Queue<Vector2Int>(path.Skip(1).Select(n => n.Item1));
                        }
                    }


                    if (!avoidPath.IsEmpty())
                    {

                        //Debug.Log("SUCCESS");
                        target = avoidPath.Dequeue();
                    }
                    else
                    {
                        var path = grid.Pathfind(snake.HeadPosition, snake.TailPosition);
                        if (path != null)
                        {
                            target = path[1].Item1;
                        }
                        else
                        {
                            target = grid.Nodes[snake.HeadPosition].FirstOrDefault().Item1;
                        }
                    }
                    Debug.Log("_________");

                    //Debug.Log(target);
                }
                if (snake.Body.Count() > 2)
                {
                    if (!grid.PredictTail(target))
                    {
                        Debug.Log("ALERT");
                        snake.TogglePause();
                    }
                }


                Vector2Int direction = target - snake.HeadPosition;
                if (direction.x > 0)
                    snake.GoEast();
                if (direction.x < 0)
                    snake.GoWest();
                if (direction.y > 0)
                    snake.GoNorth();
                if (direction.y < 0)
                    snake.GoSouth();

            });
        }



    }

    public class WeightedGrid : WeightedGraph<Vector2Int, Edge>
    {
        private bool test = true;
        private SnakeGame snake;
        private WeightedGrid mirrorGrid;
        private List<Vector2Int> tempWalls;

        private HashSet<Vector2Int> reachable = new();
        private HashSet<Vector2Int> unreachable = new();

        private string isMirror = "NOTMIRROR";

        private static Vector2Int[] directions =
        {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

        public Dictionary<Vector2Int, bool> walls;

        private Vector2 midpoint;


        public WeightedGrid(SnakeGame _snake)
        {
            snake = _snake;
            CreateGrid();
            mirrorGrid = new WeightedGrid(this);
        }

        public WeightedGrid(WeightedGrid _grid)
        {
            snake = _grid.snake;
            CreateGrid();

        }

        public void CreateGrid()
        {

            walls = new();
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
                bool isWall = (cell == SnakeGame.Cell.None) || ((cell == SnakeGame.Cell.Snake && (v != snake.HeadPosition && v != snake.TailPosition)));
                if (isWall != walls[v])
                {
                    walls[v] = isWall;
                    SetWall(v);
                }
            }
            RemoveDeadEnds(snake.HeadPosition, snake.TailPosition);


            if (mirrorGrid != null)
            {
                mirrorGrid.UpdateGrid();
            }
        }

        private void RemoveDeadEnds(Vector2Int headPosition, Vector2Int tailPosition)
        {
            if (walls[headPosition])
            {
                walls[headPosition] = false;
                SetWall(headPosition);
            }

            if (walls[tailPosition])
            {
                walls[tailPosition] = false;
                SetWall(tailPosition);
            }


            var deadEnds = (walls.Keys.Where(w => w!=headPosition && w! != tailPosition &&  !walls[w] && !IsTraversable(w)));
            while (deadEnds.Count() > 0)
            {

                foreach (Vector2Int v in deadEnds.ToList())
                {
                    walls[v] = true;
                    SetWall(v);
                }
            }

            
            var headNeighbours = Nodes[headPosition].Where(n => walls[n.Item1] && snake.IsEmpty(n.Item1));
            if (headNeighbours.Count() > 0)
                Debug.Log("HELLO");
        }

        public void SetWall(Vector2Int v)
        {
            /*
            if (walls[v])
            {
                unreachable.Add(v);
                reachable.Remove(v);
            }
            else
            {
                reachable.Add(v);
                unreachable.Remove(v);
            }*/

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
                            // ToDo: Check if edge already exists. Maybe don't connect from node to head.

                            float toWeight = 1;
                            float fromWeight = 1;

                            if (v.x > v.y)
                            {
                                if (newLocation.x != v.x)
                                {
                                    toWeight = 10f;
                                    fromWeight = 10f;
                                }
                            }
                            else if (v.y > v.x)
                            {

                                if (newLocation.y != v.y)
                                {
                                    toWeight = 10f;
                                    fromWeight = 10f;
                                }
                            }


                            Connect(v, newLocation, 1);
                            Connect(newLocation, v, 1);
                        }
                    }
                }
            }
        }

        public bool IsTraversable(Vector2Int endLocation)
        {
            if (endLocation == snake.HeadPosition)
            {
                return true;
            }

            if (walls[endLocation])
            {
                return false;
            }



            var neighbours = Nodes[endLocation];


            if (neighbours.Count == 1)
            {
                // ToDo: How many turns will this be blocked
                if (BodyNeighbours(endLocation) > 0)
                    return false;
            }
            else if (neighbours.Count == 2)
            {
                var freeNeighbours = neighbours.Where(n => Nodes[n.Item1].Count() > 1);
                if (freeNeighbours.Count() < 2 && !neighbours.Select(n => n.Item1).Contains(snake.HeadPosition))
                {
                    return false;
                }
            }

            return true;

        }

        public Vector2Int? FoodPath()
        {

            if (IsTraversable(snake.FoodPosition))
            {
                var foodNeighbours = Nodes[snake.FoodPosition];
                var path = Pathfind(snake.HeadPosition, snake.FoodPosition);

                if (path == null) return null;



                return path[1].Item1;


            }

            return null;
        }

        public void MoveSnake(List<Vector2Int> newSnake)
        {
            UpdateGrid();
            foreach (Vector2Int body in snake.Body)
            {
                walls[body] = false;
                SetWall(body);
            }

            foreach (Vector2Int point in newSnake.Skip(1))
            {
                walls[point] = true;
                SetWall(point);
            }

            RemoveDeadEnds(newSnake[0], newSnake.Last());

        }

        public void RemoveTail(int n)
        {
            // mirrorGrid.Nodes = new Dictionary<Vector2Int, HashSet<(Vector2Int, Edge)>>(Nodes);

            List<Vector2Int> newSnake = snake.Body.ToList();

            foreach (Vector2Int pos in newSnake.TakeLast(n))
            {
                mirrorGrid.walls[pos] = false;
                mirrorGrid.SetWall(pos);
            }

            if (mirrorGrid.walls[snake.FoodPosition])
            {
                mirrorGrid.RemoveDeadEnds(snake.HeadPosition, newSnake[newSnake.Count()-1-n]);
            }

        }

        public void PredictState(List<(Vector2Int, Edge)> path)
        {
            List<Vector2Int> pathPoints = path.Select(n => n.Item1).ToList();
            pathPoints.Reverse();
            int diff = snake.Body.Count() - pathPoints.Count() - 1;
            if (diff < 0)
            {
                pathPoints = pathPoints.Take(snake.Body.Count()).ToList();
            }
            else if (diff > 0)
            {
                pathPoints.AddRange(snake.Body.Skip(1).Take(diff).ToList());
            }

            if (pathPoints.Count() != snake.Body.Count() - 1)
            {
                Debug.Log("MISMATCH");
                Debug.Log(pathPoints.Count() - snake.Body.Count() - 1);
            }

            mirrorGrid.MoveSnake(pathPoints);
        }


        private bool IsGoal(Vector2Int node, Vector2Int target)
        {
            //Debug.Log(node);
            var path = Pathfind(snake.HeadPosition, node);

            //Debug.Log(path[1]);
            PredictState(path);
            var predictedPath = mirrorGrid.Pathfind(node, target);


            /*
            int steps = 0;
            if (Nodes[target].Count() == 1)
            {
                steps = snake.Body.Count() - snake.Body.ToList().FindLastIndex(v => Vector2Int.Distance(target, v) == 1);
            }*/
            // Debug.Log(predictedPath != null);



            return predictedPath != null;
        }

        public List<(Vector2Int, Edge)> Pathfind(Vector2Int start, Vector2Int end)
        {



            var path = this.AStar(
                start, end,
                (a, b) => Mathf.Abs(b.x - a.x) + Mathf.Abs(b.y - a.y));

            return path;
        }

        public List<(Vector2Int, Edge)> AvoidUntilReachable(Vector2Int target)
        {
            int stepsRequired = 0;
            for (int i = 1; i < snake.Body.Count(); i++)
            {
                RemoveTail(i);
                var foodPath = mirrorGrid.FoodPath();

                if (foodPath.HasValue)
                {
                    // Debug.Log(foodPath.Value);
                    stepsRequired = i;
                    break;
                }
            }

            //Debug.Log(stepsRequired);

            var path = this.Dijkstra(snake.HeadPosition, node => IsGoal(node, target), 10000);
            return path;
        }
        public bool IsTailReachable(Vector2Int headPosition, Vector2Int tailPosition)
        {
            var path = Pathfind(headPosition, tailPosition);

            if (path == null)
            {
                string testString = "";
                for (int y = snake.GridSize.y - 1; y >= 0; y--)
                {
                    for (int x = 0; x < snake.GridSize.x; x++)
                    {
                        if (new Vector2Int(x, y) == tailPosition)
                        {
                            testString += " x ";
                        }
                        else if (new Vector2Int(x, y) == headPosition)
                        {
                            testString += " v ";
                        }
                        else if (walls[new Vector2Int(x, y)])
                        { testString += " o "; }
                                
                        else
                        {
                            testString += " - ";
                        }
                        

                        

                    }
                    testString += "\n";
                }
                Debug.Log(testString);
            }

            return path != null;
        }

        public bool PredictTail(Vector2Int step)
        {
            PredictState(new List<(Vector2Int, Edge)> { (snake.HeadPosition, new Edge()), (step, new Edge()) });
            return mirrorGrid.IsTailReachable(step, snake.Body.ToList()[snake.Body.Count - 2]);
        }

        private int BodyNeighbours(Vector2Int location)
        {
            int count = 0;

            for (int idx = 0; idx <= 1; idx++)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2Int newCoord = location;
                    newCoord[idx] += i;
                    if (snake.IsBody(newCoord) && newCoord != snake.HeadPosition)
                        count++;
                }

            }
            return count;
        }

    }

}

