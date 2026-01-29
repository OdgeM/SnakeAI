using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AlanZucconi.AI.BT;
using AlanZucconi.Snake;

// Keyboard
[CreateAssetMenu(fileName = "SnakeAI_Keyboard", menuName = "Snake/Examples/SnakeAI_Keyboard")]
public class SnakeAI_Keyboard : SnakeAI
{
    public override Node CreateBehaviourTree(SnakeGame Snake)
    {
        return new Action
        (
            () =>
            {
                float x = Input.GetAxis("Horizontal");
                if (x > 0) Snake.Direction = Direction.East;
                if (x < 0) Snake.Direction = Direction.West;

                float y = Input.GetAxis("Vertical");
                if (y > 0) Snake.Direction = Direction.North;
                if (y < 0) Snake.Direction = Direction.South;
            }
        );
    }
}