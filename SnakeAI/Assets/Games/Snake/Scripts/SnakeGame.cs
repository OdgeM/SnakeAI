using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

using System.Linq;

using AlanZucconi.AI.BT;
using System.Text.RegularExpressions;

namespace AlanZucconi.Snake
{
    // Freely inspired by:
    // https://becominghuman.ai/designing-ai-solving-snake-with-evolution-f3dd6a9da867
    public class SnakeGame : MonoBehaviour
    {
        [Header("World")]
        [EditorOnly]
        /// <summary>
        /// The size of the world.
        /// </summary>
        /// 
        public Vector2Int GridSize = new Vector2Int(25, 25);

        //[HideInInspector]

        [EditorOnly]
        public bool Running = false;
        [Range(0, 1)]
        public float Delay = 1f / 10f; // Delays (in seconds) between game updates.  
        public bool PauseOnDeath = false;

        [Space]
        //[Range(0, 10000)]
        [EditorOnly]
        public int MaxTicks = 10000;
        [ReadOnly]
        /// <summary>
        /// The number of steps the Snake has done without dying.
        /// </summary>
        public int Ticks = 0;
        [ReadOnly]
        /// <summary>
        /// The pieces of food the snake has eaten.
        /// </summary>
        public int Score = 0;

        [HideInInspector]
        /// <summary>
        /// The position of the food.
        /// </summary>
        public Vector2Int FoodPosition;


        [Header("Challenge")]
        [Range(0, 10)]
        /// <summary>
        /// After every piece of food, it creates this many walls in the world.
        /// </summary>
        public int WallsPerFood = 0;
        private HashSet<Vector2Int> Walls = new HashSet<Vector2Int>();

        [Header("Snake")]
        [Range(1, 23 * 23)]
        /// <summary>
        /// The starting length of the snake.
        /// </summary>
        public uint StartingLength = 1;
        [ReadOnly]
        /// <summary>
        /// <para>The direction of the snake head.</para>
        /// <seealso cref="Direction"/>
        /// </summary>
        public Direction Direction = Direction.North;
        [HideInInspector]
        /// <summary>
        /// The position of the snake head in the world.
        /// </summary>
        public Vector2Int HeadPosition;
        [HideInInspector]
        public LinkedList<Vector2Int> Body = new LinkedList<Vector2Int>();
        [Space]
        public SnakeAI AI;
        private BehaviourTree Tree;
        
        [Header("Rendering")]
        public bool Rendering = true;
        [EditorOnly]
        public Tilemap Tilemap;
        [Space]
        [EditorOnly]
        //public Tile SnakeTile;
        public SnakeSkin Skin;
        //[EditorOnly]
        //public Tile HeadTile;
        [Space]
        [EditorOnly]
        public Tile EmptyTile;
        [EditorOnly]
        public Tile WallTile;
        [Space]
        [EditorOnly]
        public Tile FoodTile;
        //[Space]
        //[EditorOnly]
        //public LineRenderer Line;

        [Header("Delegates")]
        //[EditorOnly]
        [HideInInspector]
        public UnityEvent DeathCallback; // Death
        //[EditorOnly]
        [HideInInspector]
        public UnityEvent FoodCallback; // New point

        void Start()
        {
            Time.timeScale = 1f;

            // Running on Start?
            if (Running)
            {
                StartGame();
            }

            //Running = false;
            StartCoroutine(UpdateGame());
        }

        [Button(Editor=false)]
        //void Run()
        public void StartGame()
        {
            //Time.timeScale = 1f;
            Restart();
            //StartCoroutine(UpdateGame());
            Running = true;

        }

        // Reset the snake and the world
        public void Restart()
        {
            Ticks = 0;
            Score = 0;

            Direction = Direction.North;
            HeadPosition = new Vector2Int(GridSize.x / 2, GridSize.y / 2);

            Body.Clear();
            // Starting length by Tom Benyunes --
            for (int i = 0; i < StartingLength; i++)
                Body.AddFirst(HeadPosition);
            //Body.AddFirst(HeadPosition);
            ResetFood();

            Walls.Clear();

            //AI.Snake = this;
            //AI.Initialise();
            //Tree = new BehaviourTree(AI.CreateBehaviourTree(this));
            ReloadAI();

            Redraw();
        }

        public void ReloadAI ()
        {
            Tree = new BehaviourTree(AI.CreateBehaviourTree(this));

            if (!ValidateAIPath())
                Debug.LogWarning("AI name or folder might be incorrect! Please fix before assignment submission!");
        }

        // This method checks if the scriptable object SnakeAI
        // is in the "right" folder.
        // This is to test if the students have put this in the "right" folder.
        public bool ValidateAIPath()
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(AI);

            // Is this an example AI?
            if (path.Contains("Solutions") ||
                path.Contains("Examples"))
                return true;

            // Path must have at least one of the allowed formats
            return
                ValidateAIPath_Goldsmiths(path) ||
                ValidateAIPath_GameAI(path)     ;
        }

        public bool ValidateAIPath_Goldsmiths(string path)
        {
            // Assets/Snake/AIs/Students/2018-19/azucc002/SnakeAI_azucc002.asset
            string pattern = @"Assets/Games/Snake/AIs/Goldsmiths/20\d\d-\d\d/(\w\w\w\w?\w?\d\d\d)/SnakeAI_(\w\w\w\w?\w?\d\d\d)(_resit\d)??\.asset";

            Match match = Regex.Match(path, pattern);

            // No match?
            if (!match.Success)
                return false;

            // Folder names not correct?
            if (match.Groups[1].Value != match.Groups[2].Value)
                return false;

            return true;
        }

        // Validates the path for submissions made through the Game AI course
        public bool ValidateAIPath_GameAI (string path)
        {
            // Assets/Snake/AIs/Students/Game AI/ORD000000/SnakeAI_ORD000000.asset
            //string pattern = @"Assets/Games/Snake/AIs/Game AI/ORD(\d\d\d\d\d\d)/SnakeAI_ORD(\d\d\d\d\d\d)(?:_[^.]+)?\.asset";
            string pattern = @"Assets/Games/Snake/AIs/Game AI/(\w+\d\d\d)/SnakeAI_(\w+\d\d\d)(?:_[^.]+)?\.asset";

            Match match = Regex.Match(path, pattern);

            // No match?
            if (!match.Success)
                return false;

            // Folder names not correct?
            if (match.Groups[1].Value != match.Groups[2].Value)
                return false;

            return true;
        }

        IEnumerator UpdateGame()
        {
            while (true)
            {
                // Skips a frame if not running
                if (! Running)
                {
                    yield return null;
                    continue;
                }

                bool alive = UpdateSnake();
                if (!alive)
                {
                    if (PauseOnDeath)
                    {
                        TogglePause();
                        // Wait to unpause, then restart
                        while (!Running)
                            yield return null;
                    }
                    Restart();
                }

                if (Rendering)
                    Redraw();

                // If Delay is zero, simply goes to the next frame
                yield return Delay > 0
                    ? new WaitForSeconds(Delay)
                    : null;
            }
        }


        bool UpdateSnake()
        {
            Ticks++;

            //AI.Update();
            Tree.Update();

            // Moves in the direction
            HeadPosition += Direction.ToV2I();

            
            // Simulation over?
            if (Ticks >= MaxTicks)
            {
                // Original version
                DeathCallback.Invoke();
                //Restart();
                return false;
            }
            // Collision?
            //if (!IsFree(HeadPosition) || Ticks >= MaxTicks)
            if (!IsFree(HeadPosition))
            {
                
                /*
                // Original version
                DeathCallback.Invoke();
                //Restart();
                return false;
                */

                
                // Tail correction (by Erico Underdown)
                if (HeadPosition != Body.Last.Value)
                {
                    DeathCallback.Invoke();
                    //Restart();
                    return false;
                }
                
            }

            // Updates the body
            Body.AddFirst(HeadPosition);

            // Food
            if (IsFood(HeadPosition))
            {
                // Does not remove the tail if it eats food
                FoodCallback.Invoke();
                Score++;
                bool spaceLeft = ResetFood();


                // Creates new walls
                for (int i = 0; i < WallsPerFood; i++)
                {
                    spaceLeft = CreateWall();
                    if (!spaceLeft)
                        break;
                }

                // No food was created => no more space!
                if (! spaceLeft)
                {
                    DeathCallback.Invoke();
                    //Restart();
                    return false;
                }
            }
            else
            {
                Body.RemoveLast();
            }

            return true;
        }


        //[Button(Editor = false)]
        //public void StartGame() {  Running = true;     }
        //[Button(Editor = false)]
        public void StopGame()  {  Running = false;    }

        [Button(Editor = false)]
        public void TogglePause() { Running = ! Running; }

        #region Draw
        public void Redraw()
        {
            Clear();

            DrawFood();
            DrawSnake();
        }

        void Clear()
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    Tilemap.SetTile
                    (
                        position.V3I(),
                        //ISOutOfBounds(position)
                        IsWall(position)
                            ? WallTile
                            : EmptyTile
                    );
                }
            }
        }

        void DrawFood()
        {
            Tilemap.SetTile(FoodPosition.V3I(), FoodTile);
        }

        void DrawSnake()
        {
            /*
            // Snake body
            foreach (Vector2Int position in Body)
                Tilemap.SetTile(position.V3I(), SnakeTile);

            Tilemap.SetTile(HeadPosition.V3I(), HeadTile);
            */

            // Walls
            foreach (Vector2Int position in Walls)
                Tilemap.SetTile(position.V3I(), WallTile);

            
            // Only one
            if (Body.Count == 1)
            {
                if (Direction == Direction.North) Tilemap.SetTile(HeadPosition.V3I(), Skin.HeadNorth); else
                if (Direction == Direction.South) Tilemap.SetTile(HeadPosition.V3I(), Skin.HeadSouth); else
                if (Direction == Direction.West ) Tilemap.SetTile(HeadPosition.V3I(), Skin.HeadWest ); else
                if (Direction == Direction.East ) Tilemap.SetTile(HeadPosition.V3I(), Skin.HeadEast );

                return;
            }

            LinkedListNode<Vector2Int> node = Body.First;
            do
            {
                Vector2Int position = node.Value;

                // Head
                if (node.Previous == null)
                {
                    Vector2Int next = node.Next.Value;

                    if (Above(position, next))  Tilemap.SetTile(position.V3I(), Skin.HeadNorth); else
                    if (Below(position, next))  Tilemap.SetTile(position.V3I(), Skin.HeadSouth); else
                    if (Left (position, next))  Tilemap.SetTile(position.V3I(), Skin.HeadWest ); else
                    if (Right(position, next))  Tilemap.SetTile(position.V3I(), Skin.HeadEast );
                }
                // Tail
                else if (node.Next == null)
                {
                    Vector2Int previous = node.Previous.Value;
                    
                    if (Above(position, previous)) Tilemap.SetTile(position.V3I(), Skin.TailNorth); else
                    if (Below(position, previous)) Tilemap.SetTile(position.V3I(), Skin.TailSouth); else
                    if (Left (position, previous)) Tilemap.SetTile(position.V3I(), Skin.TailWest ); else
                    if (Right(position, previous)) Tilemap.SetTile(position.V3I(), Skin.TailEast );
                }
                // Middle
                else
                {
                    Vector2Int next     = node.Next.Value;
                    Vector2Int previous = node.Previous.Value;

                    // Straight
                    if (position.x == previous.x && position.x == next.x)
                        Tilemap.SetTile(position.V3I(), Skin.Vertical);
                    else
                    if (position.y == previous.y && position.y == next.y)
                        Tilemap.SetTile(position.V3I(), Skin.Horizontal);
                    else

                    // Corners
                    if (
                        (Left(position, previous) && Above(position, next)) ||
                        (Left(position, next) && Above(position, previous)) )
                        Tilemap.SetTile(position.V3I(), Skin.NW);
                    else if (
                        (Right(position, previous) && Above(position, next)) ||
                        (Right(position, next) && Above(position, previous)))
                        Tilemap.SetTile(position.V3I(), Skin.NE);
                    else if (
                        (Below(position, previous) && Right(position, next)) ||
                        (Below(position, next) && Right(position, previous)))
                        Tilemap.SetTile(position.V3I(), Skin.SE);
                    else if (
                        (Below(position, previous) && Left(position, next)) ||
                        (Below(position, next) && Left(position, previous)))
                        Tilemap.SetTile(position.V3I(), Skin.SW);
                }


                // Next node
                node = node.Next;


            } while (node != null);
            


            //foreach (Vector2Int position in Body)
            //Vector2Int previousPosition = Body.First.Value;
            //foreach (Vector2Int position in Body)
            //{
            //if (position == previousPosition)

            //}
            //Tilemap.SetTile(position.V3I(), SnakeTile);

            //    Tilemap.SetTile(HeadPosition.V3I(), HeadTile);


            //foreach (Vector2Int position in Body)
            //{
            //    DebugDraw.Rectangle(new Vector2(position.x, position.y), 0.5f, 0.5f, Color.red, Delay);
            //}

            /*
            // Line Renderer
            if (Line != null)
            {
                var points =
                    SmoothAverage
                    (
                    Body
                    .Select(p => new Vector3(p.x, p.y, 0)
                            + new Vector3(0.5f, 0.5f, -1f)  // Graphical offset
                    ))
                    .ToArray();

                //Line.positionCount = Body.Count * 2;
                Line.positionCount = points.Length;
                Line.SetPositions(points);
                //Line.Simplify(0.001f);
            }
            */
        }
        
        // Maeks the snake line a bit smoother
        private static IEnumerable<Vector3> SmoothAverage (IEnumerable<Vector3> sequence)
        {
            Vector3 previous = sequence.First();
            foreach (Vector3 current in sequence)
            {
                yield return (previous + current) / 2f;
                yield return current;

                previous = current;
            }
        }
        
        // True if a is below b
        private static bool Below (Vector2Int a, Vector2Int b)
        {
            return a.x == b.x && a.y == b.y - 1;
        }
        // True if a is above b
        private static bool Above (Vector2Int a, Vector2Int b)
        {
            return a.x == b.x && a.y == b.y + 1;
        }
        // True if a is left of b
        private static bool Left (Vector2Int a, Vector2Int b)
        {
            return a.y == b.y && a.x == b.x - 1;
        }
        // True if a is right of b
        private static bool Right(Vector2Int a, Vector2Int b)
        {
            return a.y == b.y && a.x == b.x + 1;
        }
        
        #endregion


        #region Food
        // Finds an empty positon on the grid
        Vector2Int RandomEmptyPosition()
        {
            // Generates random positions in the world
            // until if finds an empty one
            Vector2Int position;
            do
            {
                position = new Vector2Int
                (
                    Random.Range(0, GridSize.x),
                    Random.Range(0, GridSize.y)
                );
            } while (!IsEmpty(position));

            return position;
        }

        // Returns true if the food was reset
        // Returns false if there are no spaces left in the game
        bool ResetFood()
        {
            // Is there any free cell?
            // If not, we cannot create food and we terminate the game
            if (!IsThereAnEmptyPosition())
                return false;

            FoodPosition = RandomEmptyPosition();
            return true;
        }

        // Returns true if a new wall was created
        // Returns false if there are no spaces left in the game
        bool CreateWall()
        {
            // Is there any free cell?
            // If not, we cannot create create new walls and the games terminates
            if (!IsThereAnEmptyPosition())
                return false;

            Vector2Int position = RandomEmptyPosition();
            Walls.Add(position);
            return true;
        }

        // Returns true if there is at least a free position in the game
        // If not, returns false.
        // We use this to check if there is space to create food.
        bool IsThereAnEmptyPosition ()
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    // One empty position was found!
                    if (IsEmpty(position))
                        return true;
                }
            }

            // No empty positions
            return false;
        }
        #endregion



        #region CheckTiles
        public bool IsOutOfBounds(Vector2Int position)
        {
            // Out of bounds
            if (position.x <= 0 || position.x >= GridSize.x - 1)
                return true;
            if (position.y <= 0 || position.y >= GridSize.y - 1)
                return true;

            // In bounds
            return false;
        }

        // Is food in this position?
        public bool IsFood(Vector2Int position)
        {
            return position == FoodPosition;
        }

        // Is food in this position?
        public bool IsBody(Vector2Int position)
        {
            return Body.Contains(position);
        }

        // Is this position a wall?
        public bool IsWall (Vector2Int position)
        {
            if (IsOutOfBounds(position))
                return true;

            return Walls.Contains(position);
        }

        // Is the requested position free?
        // (free = you can walk on it = empty or food)
        public bool IsFree(Vector2Int position)
        {
            // Out of bounds
            if (IsOutOfBounds(position))
                return false;

            // Overlapping body?
            if (IsBody(position))
                return false;

            // Overlapping walls?
            // (ONLY NEEDED IF WALLS ARE CREATED INSIDE THE GAME AREA!)
            if (IsWall(position))
                return false;

            // Free
            return true;
        }



        bool IsFreeNorth(Vector2Int position)
        {
            return IsFree(position + Vector2Int.up);
        }
        bool IsFreeEast(Vector2Int position)
        {
            return IsFree(position + Vector2Int.right);
        }
        bool IsFreeSouth(Vector2Int position)
        {
            return IsFree(position + Vector2Int.down);
        }
        bool IsFreeWest(Vector2Int position)
        {
            return IsFree(position + Vector2Int.left);
        }

        // True if empty (= nothing, not even food)
        public bool IsEmpty (Vector2Int position)
        {
            if (!IsFree(position))
                return false;

            // Food counts as non-empty
            if (position == FoodPosition)
                return false;

            return true;
        }


        // Is not free
        public bool IsObstacle (Vector2Int position)
        {
            return !IsFree(position);
        }


        public Vector2Int TailPosition
        {
            get
            {
                return Body.Last.Value;
            }
        }

        #endregion



        #region AbsoluteMovement
        /// <summary>
        /// <para>Moves north.</para>
        /// <seealso cref="Direction"/>
        /// </summary>
        public void GoNorth ()
        {
            Direction = Direction.North;
        }
        /// <summary>
        /// <para>Moves east.</para>
        /// <seealso cref="Direction"/>
        /// </summary>
        public void GoEast()
        {
            Direction = Direction.East;
        }
        /// <summary>
        /// <para>Moves south.</para>
        /// <seealso cref="Direction"/>
        /// </summary>
        public void GoSouth()
        {
            Direction = Direction.South;
        }
        /// <summary>
        /// <para>Moves west.</para>
        /// <seealso cref="Direction"/>
        /// </summary>
        public void GoWest()
        {
            Direction = Direction.West;
        }
        #endregion

        #region LocalMovement
        // Turn Clockwise
        /// <summary>
        /// <para>Turns the head left (clockwise).</para>
        /// <seealso cref="Direction"/>
        /// </summary>
        public void TurnRight()
        {
            Direction = Direction.Right();
        }

        // Turn Anticlockwise
        /// <summary>
        /// <para>Turns the head right (anticlockwise).</para>
        /// <seealso cref="Direction"/>
        /// </summary>
        public void TurnLeft()
        {
            Direction = Direction.Left();
        }
        #endregion



        #region AbsolutePerception
        // Absolute perception
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell above the head is free.</para>
        /// <seealso cref="GoNorth"/>
        /// </summary>
        public bool IsFreeNorth()
        {
            return IsFreeNorth(HeadPosition);
        }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell to the right of the head is free.</para>
        /// <seealso cref="GoEast"/>
        /// </summary>
        public bool IsFreeEast()
        {
            return IsFreeEast(HeadPosition);
        }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell below the head is free.</para>
        /// <seealso cref="GoSouth"/>
        /// </summary>
        public bool IsFreeSouth()
        {
            return IsFreeSouth(HeadPosition);
        }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell to the left of the head is free.</para>
        /// <seealso cref="GoWest"/>
        /// </summary>
        public bool IsFreeWest()
        {
            return IsFreeWest(HeadPosition);
        }


        // Not free
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell above the head is an obstacle (a wall or the snake body).</para>
        /// <seealso cref="GoNorth"/>
        /// </summary>
        public bool IsObstacleNorth() { return !IsFreeNorth(); }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell to the right of the head is an obstacle (a wall or the snake body).</para>
        /// <seealso cref="GoEast"/>
        /// </summary>
        public bool IsObstacleEast() { return !IsFreeEast(); }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell below the head is an obstacle (a wall or the snake body).</para>
        /// <seealso cref="GoSouth"/>
        /// </summary>
        public bool IsObstacleSouth() { return !IsFreeSouth(); }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell to the left of the head is an obstacle (a wall or the snake body).</para>
        /// <seealso cref="GoWest"/>
        /// </summary>
        public bool IsObstacleWest() { return !IsFreeWest(); }



        // Food
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell above the head contains food.</para>
        /// <seealso cref="GoNorth"/>
        /// </summary>
        public bool IsFoodNorth()
        {
            return IsFood(HeadPosition + Vector2Int.up);
        }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell to the right of the head contains food.</para>
        /// <seealso cref="GoEast"/>
        /// </summary>
        public bool IsFoodEast()
        {
            return IsFood(HeadPosition + Vector2Int.right);
        }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell below the head contains food.</para>
        /// <seealso cref="GoSouth"/>
        /// </summary>
        public bool IsFoodSouth()
        {
            return IsFood(HeadPosition + Vector2Int.down);
        }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell to the left of the head contains food.</para>
        /// <seealso cref="GoWest"/>
        /// </summary>
        public bool IsFoodWest()
        {
            return IsFood(HeadPosition + Vector2Int.left);
        }
        #endregion

        #region LocalPerception
        // Local perception
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell in front of the snake the head is free.</para>
        /// <para>That is the cell the snake will travel to if its direction is left unchanged.</para>
        /// </summary>
        public bool IsFreeAhead()
        {
            return IsFree(HeadPosition + Direction.ToV2I());
        }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the snake sees a free cell to its right.</para>
        /// <para>That is the cell the snake will travel to if it turns right.</para>
        /// <seealso cref="TurnRight"/>
        /// </summary>
        public bool IsFreeRight()
        {
            return IsFree(HeadPosition + Direction.Right().ToV2I());
        }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the snake sees a free cell to its left.</para>
        /// <para>That is the cell the snake will travel to if it turns left.</para>
        /// <seealso cref="TurnLeft"/>
        /// </summary>
        public bool IsFreeLeft()
        {
            return IsFree(HeadPosition + Direction.Left().ToV2I());
        }


        // Not free
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell in front of the snake the head is an obstacle (a wall or the snake body).</para>
        /// <para>That is the cell the snake will travel to if its direction is left unchanged.</para>
        /// </summary>
        public bool IsObstacleAhead() { return !IsFreeAhead(); }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the snake sees an obstacle to its right (a wall or the snake body).</para>
        /// <para>That is the cell the snake will travel to if it turns right.</para>
        /// <seealso cref="TurnRight"/>
        /// </summary>
        public bool IsObstacleRight() { return !IsFreeRight(); }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the snake sees an obstacle to its left (a wall or the snake body).</para>
        /// <para>That is the cell the snake will travel to if it turns left.</para>
        /// <seealso cref="TurnLeft"/>
        /// </summary>
        public bool IsObstacleLeft() { return !IsFreeLeft(); }


        // Food
        /// <summary>
        /// <para>Returns <see langword="true"/> if the cell in front of the snake the head contains food.</para>
        /// <para>That is the cell the snake will travel to if its direction is left unchanged.</para>
        /// </summary>
        public bool IsFoodAhead()
        {
            return IsFood(HeadPosition + Direction.ToV2I());
        }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the snake sees food to its right.</para>
        /// <para>That is the cell the snake will travel to if it turns right.</para>
        /// <seealso cref="TurnRight"/>
        /// </summary>
        public bool IsFoodRight()
        {
            return IsFood(HeadPosition + Direction.Right().ToV2I());
        }
        /// <summary>
        /// <para>Returns <see langword="true"/> if the snake sees food to its left.</para>
        /// <para>That is the cell the snake will travel to if it turns left.</para>
        /// <seealso cref="TurnLeft"/>
        /// </summary>
        public bool IsFoodLeft()
        {
            return IsFood(HeadPosition + Direction.Left().ToV2I());
        }
        #endregion




        #region Pathfinding
        public enum Cell
        {
            None,   // No cell / out of bound / wall
            Empty,
            Snake,
            Food
        }
        // Used for pathfinding
        public Cell this [int x, int y]
        {
            get
            {
                Vector2Int position = new Vector2Int(x, y);
                return this[position];
            }
        }
        public Cell this [Vector2Int position]
        {
            get
            {
                if (IsOutOfBounds(position))
                    return Cell.None;
                if (IsWall(position))
                    return Cell.None;
                if (IsFood(position))
                    return Cell.Food;
                if (IsBody(position))
                    return Cell.Snake;
                return Cell.Empty;
            }
        }
        // Given a position,
        // iterates over the nearby indices the snake can travel to
        public IEnumerable<Vector2Int> AvailableNeighbours (Vector2Int position)
        {
            if (IsFreeNorth(position))
                yield return position + Vector2Int.up;

            if (IsFreeEast(position))
                yield return position + Vector2Int.right;

            if (IsFreeSouth(position))
                yield return position + Vector2Int.down;

            if (IsFreeWest(position))
                yield return position + Vector2Int.left;

            // No available cells
            yield break;
        }
        #endregion



        /*
        [Button(Editor = true)]
        public void CopyData()
        {

            AI.PlotData.Data.Clear();
            AI.PlotData.Data.AddRange(AI.Data.Select(p=>new Vector2(p.x, p.y)));
            UnityEditor.EditorUtility.SetDirty(AI);
        }
        */
    }
}