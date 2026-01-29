using UnityEngine;

using AlanZucconi.AI.BT;
using AlanZucconi.Snake;

// Does nothing
[CreateAssetMenu(fileName = "SnakeAI_Idle", menuName = "Snake/Examples/SnakeAI_Idle")]
public class SnakeAI_Idle : SnakeAI
{
    public override Node CreateBehaviourTree(SnakeGame Snake)
    {
        return Action.Nothing;
    }
}