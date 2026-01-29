using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AlanZucconi.AI.BT;
using AlanZucconi.Snake;

// Tries to avoid obstacles ahead of the snake
[CreateAssetMenu(fileName = "SnakeAI_Avoid", menuName = "Snake/Examples/SnakeAI_Avoid")]
public class SnakeAI_Avoid : SnakeAI
{
    public override Node CreateBehaviourTree(SnakeGame Snake)
    {
        return new Filter
        (
            // If there is an obstacle ahead...
            Snake.IsObstacleAhead,
            // ...randomly go left or right
            new Selector
            (
                true, // Random selection
                new Action(Snake.TurnLeft),
                new Action(Snake.TurnRight)
            )
        );
    }
}