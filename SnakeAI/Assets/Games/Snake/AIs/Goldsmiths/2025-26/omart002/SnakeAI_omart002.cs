using UnityEngine;
using System.Linq;
using AlanZucconi.AI.BT;
using AlanZucconi.Snake;

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
                        Snake.IsFoodReachable,            // If food is reachable...
                        new Action(Snake.MoveTowardsFood) // ...move towards the food
                    ),

                new Action
                    (
                        () => Snake.MoveTowards
                        (
                            Snake
                            .AvailableNeighbours(Snake.TailPosition)
                            .FirstOrDefault(position => Snake.IsReachable(position))
                        )
                    )

            );


        }
    }
}
