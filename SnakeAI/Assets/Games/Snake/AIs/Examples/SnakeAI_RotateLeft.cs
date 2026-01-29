using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AlanZucconi.AI.BT;
using AlanZucconi.Snake;

// This AI always rotates left
[CreateAssetMenu(fileName = "SnakeAI_RotateLeft", menuName = "Snake/Examples/SnakeAI_RotateLeft")]
public class SnakeAI_RotateLeft : SnakeAI
{
    public override Node CreateBehaviourTree(SnakeGame Snake)
    {
        return new Action(Snake.TurnLeft);
    }
}