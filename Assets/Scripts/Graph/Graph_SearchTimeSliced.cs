using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//these enums are used as return values from each search update method
enum search_status { target_found, target_not_found, search_incomplete };



//------------------------ Graph_SearchTimeSliced -----------------------------
//
// base class to define a common interface for graph search algorithms
//-----------------------------------------------------------------------------
//template <class edge_type>
public abstract class Graph_SearchTimeSliced
{
    public enum SearchType { AStar, Dijkstra };

    SearchType m_SearchType;


    public Graph_SearchTimeSliced(SearchType type)
    {
        m_SearchType = type;
    }

    //When called, this method runs the algorithm through one search cycle. The
    //method returns an enumerated value (target_found, target_not_found,
    //search_incomplete) indicating the status of the search
    public abstract int CycleOnce();

    //returns the vector of edges that the algorithm has examined
    public abstract List<GraphEdge> GetSPT();

    //returns the total cost to the target
    public abstract double GetCostToTarget();

    //returns a list of node indexes that comprise the shortest path
    //from the source to the target
    public abstract List<int> GetPathToTarget();

    //returns the path as a list of PathEdges
    public abstract List<GraphEdge> GetPathAsPathEdges();

    public SearchType GetType(){return m_SearchType;}
};


//-------------------------- Graph_SearchAStar_TS -----------------------------
//
//  a A* class that enables a search to be completed over multiple update-steps
//-----------------------------------------------------------------------------
//template <class graph_type, class heuristic>
public class Graph_SearchAStar_TS : Graph_SearchTimeSliced
{
  
    //create typedefs for the node and edge types used by the graph
    //typedef typename graph_type::EdgeType Edge;
    //typedef typename graph_type::NodeType Node;


    SparseGraph m_Graph;
    AStarHeuristic m_Heuristic;

    //indexed into my node. Contains the 'real' accumulative cost to that node
    List<double> m_GCosts;

    //indexed into by node. Contains the cost from adding m_GCosts[n] to
    //the heuristic cost from n to the target node. This is the vector the
    //iPQ indexes into.
    List<double> m_FCosts;

    List<GraphEdge> m_ShortestPathTree;
    List<GraphEdge> m_SearchFrontier;

    int m_iSource;
    int m_iTarget;

    //create an indexed priority queue of nodes. The nodes with the
    //lowest overall F cost (G+H) are positioned at the front.
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

    //When called, this method pops the next node off the PQ and examines all
    //its edges. The method returns an enumerated value (target_found,
    //target_not_found, search_incomplete) indicating the status of the search
    public override int CycleOnce() 
    {
        //if the PQ is empty the target has not been found
        if (m_pPQ.empty())
        {
            return (int)search_status.target_not_found;
        }

        //get lowest cost node from the queue
        int NextClosestNode = m_pPQ.Pop();

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

            //if this edge has never been on the frontier make a note of the cost
            //to get to the node it points to, then add the edge to the frontier
            //and the destination node to the PQ.
            if (m_SearchFrontier[edge.To()] is null)
            {
                m_FCosts[edge.To()] = GCost + HCost;
                m_GCosts[edge.To()] = GCost;

                m_pPQ.insert(edge.To());

                m_SearchFrontier[edge.To()] = edge;
            }

            //else test to see if the cost to reach the destination node via the
            //current node is cheaper than the cheapest cost found so far. If
            //this path is cheaper, we assign the new cost to the destination
            //node, update its entry in the PQ to reflect the change and add the
            //edge to the frontier
            else if ((GCost < m_GCosts[edge.To()]) && (m_ShortestPathTree[edge.To()] is null))
            //else if ((NewCost < m_CostToThisNode[edge.To()]) && (m_ShortestPathTree[edge.To()] == 0))
            {
                m_FCosts[edge.To()] = GCost + HCost;
                m_GCosts[edge.To()] = GCost;

                //because the cost is less than it was previously, the PQ must be
                //re-sorted to account for this.
                m_pPQ.ChangePriority(edge.To());

                m_SearchFrontier[edge.To()] = edge;
            }
        }

        //there are still nodes to explore
        return (int)search_status.search_incomplete;
    }

    //returns the vector of edges that the algorithm has examined
    public override List<GraphEdge> GetSPT(){return m_ShortestPathTree;}

    //returns a vector of node indexes that comprise the shortest path
    //from the source to the target
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

    //returns the path as a list of PathEdges (INCOMPLETE)
    public override List<GraphEdge> GetPathAsPathEdges() 
    {
        List<GraphEdge> path = new List<GraphEdge>();

        //just return an empty path if no target or no path found
        if (m_iTarget < 0) return path;

        int nd = m_iTarget;

        //while ((nd != m_iSource) && !(m_ShortestPathTree[nd] is null))
        //{
        //    path.push_front(PathEdge(m_Graph.GetNode(m_ShortestPathTree[nd]->From()).Pos(), m_Graph.GetNode(m_ShortestPathTree[nd]->To()).Pos(),
        //                             m_ShortestPathTree[nd]->Flags(),
        //                             m_ShortestPathTree[nd]->IDofIntersectingEntity()));

        //    nd = m_ShortestPathTree[nd]->From();
        //}

        return path;
    }

    //returns the total cost to the target
    public override double GetCostToTarget()
    {
        return m_GCosts[m_iTarget];
    }
};