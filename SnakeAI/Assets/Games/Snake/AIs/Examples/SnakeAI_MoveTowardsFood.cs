using UnityEngine;

using AlanZucconi.AI.BT;
using AlanZucconi.Snake;
using System.Linq;

[CreateAssetMenu
(
    fileName = "SnakeAI_MoveTowardsFood",
    menuName = "Snake/Examples/SnakeAI_MoveTowardsFood"
)]
public class SnakeAI_MoveTowardsFood : SnakeAI
{
    public override Node CreateBehaviourTree(SnakeGame Snake)
    {
        return new Action(() => Snake.MoveTowardsFood());
    }
}