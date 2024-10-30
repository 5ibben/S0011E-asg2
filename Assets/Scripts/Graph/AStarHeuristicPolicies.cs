using UnityEngine;

public abstract class AStarHeuristic
{
    public abstract double Calculate(Graph G, int nd1, int nd2);
}

class AStarHeuristic_Euclid : AStarHeuristic
{
    public AStarHeuristic_Euclid() { }

    //calculate the straight line distance from node nd1 to node nd2
    public override double Calculate(Graph G, int nd1, int nd2)
    {
        return (((GraphNode_Demo)((SparseGraph)G).GetNode(nd1)).GetPos() - ((GraphNode_Demo)((SparseGraph)G).GetNode(nd2)).GetPos()).magnitude;
    }
};

//-----------------------------------------------------------------------------
//this uses the euclidian distance but adds in an amount of noise to the 
//result. You can use this heuristic to provide imperfect paths. This can
//be handy if you find that you frequently have lots of agents all following
//each other in single file to get from one place to another
//-----------------------------------------------------------------------------
class AStarHeuristic_Noisy_Euclidian : AStarHeuristic
{
    public AStarHeuristic_Noisy_Euclidian() { }

    //calculate the straight line distance from node nd1 to node nd2
    public override double Calculate(Graph G, int nd1, int nd2)
    {
        return (((GraphNode_Demo)((SparseGraph)G).GetNode(nd1)).GetPos() - ((GraphNode_Demo)((SparseGraph)G).GetNode(nd2)).GetPos()).magnitude * Random.Range(0.9f, 1.1f);
    }
};

//-----------------------------------------------------------------------------
//you can use this class to turn the A* algorithm into Dijkstra's search.
//this is because Dijkstra's is equivalent to an A* search using a heuristic
//value that is always equal to zero.
//-----------------------------------------------------------------------------
class AStarHeuristic_Dijkstra : AStarHeuristic
{
    public AStarHeuristic_Dijkstra() {  }
    public override double Calculate(Graph G, int nd1, int nd2)
    {
        return 0;
    }
};

class AStarHeuristic_Manhattan : AStarHeuristic
{
    public AStarHeuristic_Manhattan() { }
    public override double Calculate(Graph G, int nd1, int nd2)
    {
        Vector3 distVec = ((GraphNode_Demo)((SparseGraph)G).GetNode(nd1)).GetPos() - ((GraphNode_Demo)((SparseGraph)G).GetNode(nd2)).GetPos();
        return Mathf.Abs(distVec.x) + Mathf.Abs(distVec.y);
    }
};