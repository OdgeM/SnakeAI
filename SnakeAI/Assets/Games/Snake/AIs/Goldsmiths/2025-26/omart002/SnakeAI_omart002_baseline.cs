using UnityEngine;
using System.Linq;
using AlanZucconi.AI.BT;
using AlanZucconi.Snake;

namespace Snake.omart002_baseline
{
    [CreateAssetMenu(fileName = "SnakeAI_omart002_baseline",
                     menuName = "Snake/2025-26/SnakeAI_omart002_baseline")]
    public class SnakeAI_omart002_baseline : SnakeAI
    {
        public override Node CreateBehaviourTree(SnakeGame Snake)
        {

            return new Filter
                (
                    Snake.IsFoodReachable,            // If food is reachable...
                    new Action(Snake.MoveTowardsFood) // ...move towards the food
                );



        }
    }
}


