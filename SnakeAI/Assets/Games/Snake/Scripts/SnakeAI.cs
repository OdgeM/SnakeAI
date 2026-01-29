using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AlanZucconi.AI.BT;
using AlanZucconi.Data;

namespace AlanZucconi.Snake
{
    public abstract class SnakeAI : ScriptableObject
    {
        [Header("Student Data")]
        public string StudentLogin = "yourlogin";
        public string StudentName = "FirstName LastName";
        public string StudentEmail = "youremail@gold.ac.uk";

        [Header("AI Data")]
        public string AIName = "AI Name";
        [TextArea(1, 5)]
        public string AIDescription = "The description of your AI";

        //[HideInInspector]
        //public SnakeGame Snake;

        [Header("Results")]
        //[HideInInspector]
        //[ScatterPlot]
        //public List<Vector2Int> Data = new List<Vector2Int>();
        [ScatterPlot(LabelX = "ticks", LabelY = "points")]
        public PlotData PlotData = new PlotData();

        [Header("Challenge")]
        [ScatterPlot(LabelX = "ticks", LabelY = "points")]
        public PlotData PlotDataChallenge = new PlotData();

        //[HideInInspector]
        //public BehaviourTree Tree;

        /*
        public void Initialise ()
        {
            Tree = new BehaviourTree(CreateBT());
        }

        public void Update()
        {
            Tree.Update();
        }
        */
        public virtual Node CreateBehaviourTree(SnakeGame snake)
        {
            return null;
        }
    }
}