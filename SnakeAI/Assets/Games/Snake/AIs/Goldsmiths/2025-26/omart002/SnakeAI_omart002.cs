using UnityEngine;

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
            return Action.Nothing;
        }
    }
}
