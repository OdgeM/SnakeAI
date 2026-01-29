using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using AlanZucconi.AI.PF;

public class PathfindingTest : MonoBehaviour
{

    //public class GraphString : IMap<string>
    public class GraphString : IPathfinding<string>
    {
        public IEnumerable<string> Outgoing(string node)
        {
            throw new NotImplementedException();
        }
    }

    public class GraphInt : IPathfinding<int>
    {
        public  IEnumerable<int> Outgoing(int node)
        {
            yield return node + 1;
        }
    }

    #region TestArray
    public class GraphArray : IPathfinding<int>
    {
        public int[][] Array;
        
        public IEnumerable<int> Outgoing(int i)
        {
            return Array[i];
        }
    }
    #endregion

    // Use this for initialization
    void Start () {
        //GraphB b = new GraphB();
        //foreach (int i in b.Outstar(1))
        //{
        //    Debug.Log(i);
        //}

        //foreach (int i in b.)

        //string? z;

        //int? x = 1;
        //var y = x;

        GraphInt g = new GraphInt();
        List<int> path = g.BreadthFirstSearch(5, 10);
        foreach (int i in path)
        {
            Debug.Log(i);
        }

        
        Graph<string> graph = new Graph<string>();
        graph.Connect("a", "b");
        graph.Connect("a", "c");
        graph.Connect("a", "e");

        graph.Connect("b", "f");

        graph.Connect("c", "d");
        graph.Connect("c", "g");

        graph.Connect("d", "g");
        graph.Connect("d", "f");

        graph.Connect("e", "f");
        graph.Connect("e", "h");

        graph.Connect("f", "z");

        graph.Connect("g", "z");

        graph.Connect("h", "z");

        
        List<string> p = graph.BreadthFirstSearch("a", "z");
        Debug.Log("a to z:");
        foreach (string node in p)
            Debug.Log("\t" + node);


        p = graph.BreadthFirstSearch("a", "a");
        Debug.Log("a to a:");
        foreach (string node in p)
            Debug.Log("\t" + node);

        p = graph.BreadthFirstSearch("a", "x");
        Debug.Log(p);

        p = graph.BreadthFirstSearch("x", "a");
        Debug.Log(p);



        Grid2D grid = new Grid2D(new Vector2Int(10, 10));
        grid.SetWall(new Vector2Int(8, 9));
        grid.SetWall(new Vector2Int(8, 8));
        grid.SetWall(new Vector2Int(8, 7));
        grid.SetWall(new Vector2Int(8, 6));
        grid.SetWall(new Vector2Int(8, 5));
        var xxx = grid.BreadthFirstSearch(new Vector2Int(0, 0), new Vector2Int(9, 9));
        foreach (Vector2Int i in xxx)
            Debug.Log("\t" + i);



        // Test array
        GraphArray graphArray = new GraphArray
        {
            Array =
            new int[][]
            {
                /* 0 */ new int [] { 1, 4    },
                /* 1 */ new int [] { 0, 2, 4 },
                /* 2 */ new int [] { 1, 3    },
                /* 3 */ new int [] { 2, 4, 5 },
                /* 4 */ new int [] { 0, 1, 3 },
                /* 5 */ new int [] { 3       }
            }
        };
        foreach (int i in graphArray.BreadthFirstSearch(0, 5))
            Debug.Log("\t" + i);




        // Test for Dijstra and A* via conversion
        UnitCostGraph<int> g_unit = g.ToWeightedGraph();
        List<(int, Edge)> unit_path = g_unit.Dijkstra(5, 10);
        foreach ((int i, Edge e) in unit_path)
            Debug.Log($"{i}\t{e}");





        // Test for Weighted Graph
        WeightedGraph<string, Edge> wgraph = new WeightedGraph<string, Edge>();
        wgraph.Connect("a", "b", 2);
        wgraph.Connect("a", "c", 1);
        wgraph.Connect("a", "e", 3);

        wgraph.Connect("b", "f", 5);

        wgraph.Connect("c", "d", 1);
        wgraph.Connect("c", "g", 2);

        wgraph.Connect("d", "g", 6);
        wgraph.Connect("d", "f", 2);

        wgraph.Connect("e", "f", 3);
        wgraph.Connect("e", "h", 2);

        wgraph.Connect("f", "z", 1);

        wgraph.Connect("g", "z", 5);

        wgraph.Connect("h", "z", 1);

        List<(string, Edge)> wp = wgraph.Dijkstra("a", "z");
        Debug.Log("a to z:");
        foreach ((string node, Edge edge) in wp)
            Debug.Log($"\t{node}\t{edge}");


        List<(string, Edge)> wpstar = wgraph.AStar("a", "z", (n1, n2) => 0);
        Debug.Log("a to z:");
        foreach ((string node, Edge edge) in wpstar)
            Debug.Log($"\t{node}\t{edge}");
    }
}
