using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AlanZucconi.AI.BT;
using AlanZucconi.Snake;

// Picks a random direction
[CreateAssetMenu(fileName = "SnakeAI_Random", menuName = "Snake/Examples/SnakeAI_Random")]
public class SnakeAI_Random : SnakeAI
{
    public override Node CreateBehaviourTree(SnakeGame Snake)
    {
        return new Selector
        (
            true, // Random selection
            new Action(Snake.GoNorth),
            new Action(Snake.GoEast),
            new Action(Snake.GoSouth),
            new Action(Snake.GoWest)
        );
    }
}