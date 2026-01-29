using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AlanZucconi.AI.PF;



public class NoughtsAndCrosses : MonoBehaviour
{

    public enum Symbol
    {
        None,
        Nought,
        Cross
    }

    // The state of a game of Noughts And Crosses
    public struct State
    {
        // Bottom row
        public Symbol S_00;
        public Symbol S_10;
        public Symbol S_20;

        // Medium row
        public Symbol S_01;
        public Symbol S_11;
        public Symbol S_21;

        // Top row
        public Symbol S_02;
        public Symbol S_12;
        public Symbol S_22;

        public bool CrossTurn;

        private Symbol Symbol
        {
            get
            {
                return CrossTurn ? Symbol.Cross : Symbol.Nought;
            }
        }

        public IEnumerable <State> Outgoing ()
        {
            if (S_00 == Symbol.None)
            {
                State state = this;
                state.S_00 = Symbol;
                state.CrossTurn = !state.CrossTurn;
                yield return state;
            }

            if (S_10 == Symbol.None)
            {
                State state = this;
                state.S_10 = Symbol;
                state.CrossTurn = !state.CrossTurn;
                yield return state;
            }

            if (S_20 == Symbol.None)
            {
                State state = this;
                state.S_20 = Symbol;
                state.CrossTurn = !state.CrossTurn;
                yield return state;
            }





            if (S_01 == Symbol.None)
            {
                State state = this;
                state.S_01 = Symbol;
                state.CrossTurn = !state.CrossTurn;
                yield return state;
            }

            if (S_11 == Symbol.None)
            {
                State state = this;
                state.S_11 = Symbol;
                state.CrossTurn = !state.CrossTurn;
                yield return state;
            }

            if (S_21 == Symbol.None)
            {
                State state = this;
                state.S_21 = Symbol;
                state.CrossTurn = !state.CrossTurn;
                yield return state;
            }



            if (S_02 == Symbol.None)
            {
                State state = this;
                state.S_02 = Symbol;
                state.CrossTurn = !state.CrossTurn;
                yield return state;
            }

            if (S_12 == Symbol.None)
            {
                State state = this;
                state.S_12 = Symbol;
                state.CrossTurn = !state.CrossTurn;
                yield return state;
            }

            if (S_22 == Symbol.None)
            {
                State state = this;
                state.S_22 = Symbol;
                state.CrossTurn = !state.CrossTurn;
                yield return state;
            }

            yield break;
        }


        public override string ToString ()
        {
            return string.Format("{0}\t{1}\t{2}\n{3}\t{4}\t{5}\n{6}\t{7}\t{8}\n{9}",
                S_02, S_12, S_22,
                S_01, S_11, S_21,
                S_00, S_10, S_20,
                "Cross Moves Next: " + CrossTurn
                );
        }
    }


    public class NoughtsAndCrossesFinding : IPathfinding<State>
    {
        public IEnumerable<State> Outgoing(State state)
        {
            return state.Outgoing();
        }
    }

    // Use this for initialization
    void Start ()
    {
        State stateStart = new State();

        State stateGoal = new State();
        stateGoal.S_11 = Symbol.Cross;
        stateGoal.S_01 = Symbol.Nought;
        stateGoal.S_02 = Symbol.Cross;
        stateGoal.S_00 = Symbol.Nought;
        stateGoal.CrossTurn = false;

        //Debug.Log(stateGoal);
        //foreach (State s in stateStart.Outgoing())
        //foreach (State s in stateGoal.Outgoing())
        //    Debug.Log(s);

        
        
        NoughtsAndCrossesFinding game = new NoughtsAndCrossesFinding();
        List<State> path = game.BreadthFirstSearch(stateStart, stateGoal);

        foreach (State state in path)
            Debug.Log(state);
    }
}
