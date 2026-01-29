using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.Snake
{
    public class Automation : MonoBehaviour
    {
        [Header("Game Parameteres")]
        [EditorOnly]
        public SnakeGame Snake;
        [Range(0f,1f)]
        public float Delay = 0f;

        //[Header("Challenge")]
        //[EditorOnly]
        private bool IsChallenge = false;

        [Header("Simulation Parameteres")]
        public bool PerformClassicTest = true; // If false, does not perform the classic test
        public bool PerformChallengeTest = true; // If false, does not perform the challenge test

        [Range(1,1000)]
        public int TestsPerAI = 100;
        public bool Rendering = true;
        [EditorOnly]
        public bool ClearData = true;
        [Space]
        public List<SnakeAI> AIs;


        // Use this for initialization
        //void Start()
        [Button(Editor=false)]
        void Run ()
        {
            Snake.DeathCallback.AddListener(SimulationDone);

            StartCoroutine(Automate());
        }
        
        IEnumerator Automate ()
        {
            // Runs as fast as possible
            QualitySettings.vSyncCount = 0; // Set vSyncCount to 0 so that using .targetFrameRate is enabled.
            Application.targetFrameRate = -1; // No target framerate

            foreach (SnakeAI ai in AIs)
            {
                Debug.Log("Testing AI: [" + ai.AIName + "]...");

                // Test twice: classic and challenge
                List<bool> tests = new List<bool>();
                if (PerformClassicTest)
                    tests.Add(false); // false: classic
                if (PerformChallengeTest)
                    tests.Add(true); // true: challenge

                //for (int j = 0; j <= 1; j++)
                foreach (bool test in tests)
                {
                    IsChallenge = test;
                    //IsChallenge = j == 1;

                    if (ClearData)
                    {
                        if (!IsChallenge)
                            ai.PlotData.Data.Clear();
                        else
                            ai.PlotDataChallenge.Data.Clear();
                    }

                    for (int i = 0; i < TestsPerAI; i++)
                    {
                        Debug.Log("\tSimulation " + i + "\tof " + TestsPerAI + "...");

                        Reset(ai);
                        Snake.StartGame();

                        StartSimulation();
                        yield return new WaitWhile(() => Running); // Wait until simulation done
                        Snake.StopGame();
                    }
                }
            }

            

            Debug.Log("DONE!");
        }


        public void Reset(SnakeAI ai)
        {
            Snake.AI = ai;

            Snake.Restart();

            Snake.Delay = Delay;
            Snake.Rendering = Rendering;
            Snake.PauseOnDeath = false;
            Snake.StartingLength = 1;

            if (IsChallenge)
                Snake.WallsPerFood = 1;
            else
                Snake.WallsPerFood = 0;
        }



        private bool Running = false;
        public void StartSimulation ()
        {
            Running = true;
        }
        public void SimulationDone ()
        {
            CollectStats(Snake.AI);
            Running = false;
        }


        public void CollectStats (SnakeAI ai)
        {
            if (! IsChallenge)
                ai.PlotData.Add(new Vector2(Snake.Ticks, Snake.Body.Count));
            else
                ai.PlotDataChallenge.Add(new Vector2(Snake.Ticks, Snake.Body.Count));

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(ai);
#endif
        }
    }
}