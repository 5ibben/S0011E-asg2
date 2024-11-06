using System.Collections.Generic;
using UnityEngine;

enum search_status { target_found, target_not_found, search_incomplete };
public abstract class Graph_SearchTimeSliced// : MonoBehaviour
{
    public enum SearchType { AStar, Dijkstra };

    SearchType m_SearchType;

    public Graph_SearchTimeSliced(SearchType type)
    {
        m_SearchType = type;
    }

    public void CleanUP()
    {
        Visualizer.ClearPathFinderMarkers();
    }

    //When called, this method runs the algorithm through one search cycle.
    public abstract int CycleOnce();

    //returns the vector of edges that the algorithm has examined
    public abstract List<GraphEdge> GetSPT();

    //returns the total cost to the target
    public abstract double GetCostToTarget();

    //returns a list of node indexes that comprise the shortest path from the source to the target
    public abstract List<int> GetPathToTarget();

    //returns the path as a list of PathEdges
    public abstract List<PathEdge> GetPathAsPathEdges();

    new public SearchType GetType(){return m_SearchType;}
};


//-------------------------- Graph_SearchAStar_TS -----------------------------
//
//  a A* class that enables a search to be completed over multiple update-steps
//-----------------------------------------------------------------------------
public class Graph_SearchAStar_TS : Graph_SearchTimeSliced
{
    SparseGraph m_Graph;
    AStarHeuristic m_Heuristic;

    //indexed into my node. Contains the 'real' accumulative cost to that node
    List<double> m_GCosts;

    //indexed into by node. Contains the cost from adding m_GCosts[n] to the heuristic cost from n to the target node. This is the vector the iPQ indexes into.
    List<double> m_FCosts;

    List<GraphEdge> m_ShortestPathTree;
    List<GraphEdge> m_SearchFrontier;

    int m_iSource;
    int m_iTarget;

    //create an indexed priority queue of nodes. The nodes with the lowest overall F cost (G+H) are positioned at the front.
    IndexedPriorityQueueLow m_pPQ;

    public Graph_SearchAStar_TS(SparseGraph G, AStarHeuristic heuristic, int source, int target):base(SearchType.AStar)
    {
        m_Graph = G;
        m_Heuristic = heuristic;
        m_iSource = source;
        m_iTarget = target;
        m_ShortestPathTree = new List<GraphEdge>(m_Graph.NumNodes());
        m_SearchFrontier = new List<GraphEdge>(m_Graph.NumNodes());
        m_GCosts = new List<double>(m_Graph.NumNodes());
        m_FCosts = new List<double>(m_Graph.NumNodes());
        for (int i = 0; i < m_Graph.NumNodes(); i++)
        {
            m_ShortestPathTree.Add(null);
            m_SearchFrontier.Add(null);
            m_GCosts.Add(0);
            m_FCosts.Add(0);
        }

        //create the PQ   
        m_pPQ = new IndexedPriorityQueueLow(m_FCosts, m_Graph.NumNodes());

        //put the source node on the queue
        m_pPQ.insert(m_iSource);
    }

    //When called, this method pops the next node off the PQ and examines all its edges.
    public override int CycleOnce() 
    {
        //if the PQ is empty the target has not been found
        if (m_pPQ.empty())
        {
            return (int)search_status.target_not_found;
        }

        //get lowest cost node from the queue
        int NextClosestNode = m_pPQ.Pop();

        if (Config.visualizePathfinding)
        {
            Visualizer.AddPathFinderMarker(((GraphNode_Demo)Map.Graph().GetNode(NextClosestNode)).GetPos());
        }

        //put the node on the SPT
        m_ShortestPathTree[NextClosestNode] = m_SearchFrontier[NextClosestNode];

        //if the target has been found exit
        if (NextClosestNode == m_iTarget)
        {
            return (int)search_status.target_found;
        }

        foreach (GraphEdge edge in m_Graph.GetNodeEdges(NextClosestNode))
        {
            //calculate the heuristic cost from this node to the target (H)                       
            double HCost = m_Heuristic.Calculate(m_Graph, m_iTarget, edge.To());

            //calculate the 'real' cost to this node from the source (G)
            double GCost = m_GCosts[NextClosestNode] + edge.Cost();

            //if this edge has never been on the frontier make a note of the costto get to the node it points to, then add the edge to the frontier and the destination node to the PQ.
            if (m_SearchFrontier[edge.To()] is null)
            {
                m_FCosts[edge.To()] = GCost + HCost;
                m_GCosts[edge.To()] = GCost;

                m_pPQ.insert(edge.To());

                m_SearchFrontier[edge.To()] = edge;
            }

            //else test to see if the cost to reach the destination node via the current node is cheaper than the cheapest cost found so far.
            else if ((GCost < m_GCosts[edge.To()]) && (m_ShortestPathTree[edge.To()] is null))
            {
                m_FCosts[edge.To()] = GCost + HCost;
                m_GCosts[edge.To()] = GCost;

                //because the cost is less than it was previously, the PQ must be re-sorted to account for this.
                m_pPQ.ChangePriority(edge.To());

                m_SearchFrontier[edge.To()] = edge;
            }
        }

        //there are still nodes to explore
        return (int)search_status.search_incomplete;
    }

    //returns the vector of edges that the algorithm has examined
    public override List<GraphEdge> GetSPT(){return m_ShortestPathTree;}

    //returns a vector of node indexes that comprise the shortest path from the source to the target
    public override List<int> GetPathToTarget() 
    {
        List<int> path = new List<int>();

        //just return an empty path if no target or no path found
        if (m_iTarget < 0) return path;

        int nd = m_iTarget;

        path.Add(nd);

        while ((nd != m_iSource) && !(m_ShortestPathTree[nd] is null))
        {
            nd = m_ShortestPathTree[nd].From();

            path.Add(nd);
        }

        return path;
    }

    //returns the path as a list of PathEdges
    public override List<PathEdge> GetPathAsPathEdges() 
    {
        List<PathEdge> path = new List<PathEdge>();

        //just return an empty path if no target or no path found
        if (m_iTarget < 0) return path;

        int nd = m_iTarget;

        while ((nd != m_iSource) && !(m_ShortestPathTree[nd] is null))
        {
            Vector2 vFrom = ((GraphNode_Demo)m_Graph.GetNode(m_ShortestPathTree[nd].From())).GetPos();
            Vector2 vTo = ((GraphNode_Demo)m_Graph.GetNode(m_ShortestPathTree[nd].To())).GetPos();
            path.Add(new PathEdge(vFrom, vTo, 0, -1));

            nd = m_ShortestPathTree[nd].From();
        }

        return path;
    }

    //returns the total cost to the target
    public override double GetCostToTarget()
    {
        return m_GCosts[m_iTarget];
    }
};
public class Graph_SearchDijkstra_TS : Graph_SearchTimeSliced
{
    SparseGraph m_Graph;

    List<double> m_CostToThisNode;
    List<GraphEdge> m_ShortestPathTree;
    List<GraphEdge> m_SearchFrontier;

    int m_iSource;
    int m_iTarget;

    IndexedPriorityQueueLow pq;
    Graph_Serach_Termination_Condition termination_Condition;

    public  Graph_SearchDijkstra_TS(SparseGraph graph, int source, Graph_Serach_Termination_Condition termination) :base(SearchType.Dijkstra)
    {
        m_Graph = graph;
        m_iSource = source;
        m_iTarget = -1;
        termination_Condition = termination;
        m_ShortestPathTree = new List<GraphEdge>(m_Graph.NumNodes());
        m_SearchFrontier = new List<GraphEdge>(m_Graph.NumNodes());
        m_CostToThisNode = new List<double>(m_Graph.NumNodes());
        for (int i = 0; i<m_Graph.NumNodes(); i++)
        {
            m_ShortestPathTree.Add(null);
        }
        for (int i = 0; i < m_Graph.NumNodes(); i++)
        {
            m_SearchFrontier.Add(null);
        }
        for (int i = 0; i < m_Graph.NumNodes(); i++)
        {
            m_CostToThisNode.Add(0);
        }

        //create the PQ   
        pq = new IndexedPriorityQueueLow(m_CostToThisNode, m_Graph.NumNodes());

        //put the source node on the queue
        pq.insert(m_iSource);
    }

    public override int CycleOnce()
    {
        //if the PQ is empty the target has not been found
        if (pq.empty())
        {
            return (int)search_status.target_not_found;
        }

        int NextClosestNode = pq.Pop();

        if (Config.visualizePathfinding)
        {
            Visualizer.AddPathFinderMarker(((GraphNode_Demo)Map.Graph().GetNode(NextClosestNode)).GetPos());
        }

        //move this edge from the frontier to the shortest path tree
        m_ShortestPathTree[NextClosestNode] = m_SearchFrontier[NextClosestNode];

        //if the target has been found exit
        if (termination_Condition.isSatisfied(m_Graph, NextClosestNode))
        {
            m_iTarget = NextClosestNode;
            return (int)search_status.target_found;
        }

        //now to relax the edges for each edge connected to the next closest node
        foreach (GraphEdge edge in m_Graph.GetNodeEdges(NextClosestNode))
        {
            //the total cost to the node this edge points to is the cost to the current node plus the cost of the edge connecting them.
            double NewCost = m_CostToThisNode[NextClosestNode] + edge.Cost();

            //if this edge has never been on the frontier make a note of the cost to get to the node it points to, then add the edge to the frontier and the destination node to the PQ.
            if (m_SearchFrontier[edge.To()] is null)
            {
                m_CostToThisNode[edge.To()] = NewCost;

                pq.insert(edge.To());

                m_SearchFrontier[edge.To()] = edge;
            }

            //else test to see if the cost to reach the destination node via the current node is cheaper than the cheapest cost found so far.
            else if ((NewCost < m_CostToThisNode[edge.To()]) && (m_ShortestPathTree[edge.To()] is null))
            {
                m_CostToThisNode[edge.To()] = NewCost;

                //because the cost is less than it was previously, the PQ must be re-sorted to account for this.
                pq.ChangePriority(edge.To());

                m_SearchFrontier[edge.To()] = edge;
            }
        }
        //there are still nodes to explore
        return (int)search_status.search_incomplete;
    }

    //returns the vector of edges that the algorithm has examined
    public override List<GraphEdge> GetSPT() { return m_ShortestPathTree; }

    //returns a list of node indexes that comprise the shortest path from the source to the target
    public override List<int> GetPathToTarget()
    {
        List<int> path = new List<int>();

        //just return an empty path if no target or no path found
        if (m_iTarget < 0) return path;

        int nd = m_iTarget;

        path.Add(nd);

        while ((nd != m_iSource) && !(m_ShortestPathTree[nd] is null))
        {
            nd = m_ShortestPathTree[nd].From();

            path.Add(nd);
        }

        return path;
    }

    //returns the path as a list of PathEdges
    public override List<PathEdge> GetPathAsPathEdges()
    {
        List<PathEdge> path = new List<PathEdge>();

        //just return an empty path if no target or no path found
        if (m_iTarget < 0) return path;

        int nd = m_iTarget;

        while ((nd != m_iSource) && !(m_ShortestPathTree[nd] is null))
        {
            Vector2 vFrom = ((GraphNode_Demo)m_Graph.GetNode(m_ShortestPathTree[nd].From())).GetPos();
            Vector2 vTo = ((GraphNode_Demo)m_Graph.GetNode(m_ShortestPathTree[nd].To())).GetPos();
            path.Add(new PathEdge(vFrom, vTo, 0, -1));

            nd = m_ShortestPathTree[nd].From();
        }

        return path;
    }

    //returns the total cost to the target
    public override double GetCostToTarget()
    {
        return m_CostToThisNode[m_iTarget];
    }
}
