using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph_SearchAStar// : MonoBehaviour
{
    SparseGraph m_Graph;
    AStarHeuristic m_Heuristic;

    //indexed into my node. Contains the 'real' accumulative cost to that node
    List<double> m_GCosts;

    //indexed into by node. Contains the cost from adding m_GCosts[n] to
    //the heuristic cost from n to the target node. This is the vector the
    //iPQ indexes into.
    List<double> m_FCosts;

    //this vector contains the edges that comprise the shortest path tree -
    //a directed subtree of the graph that encapsulates the best paths from 
    //every node on the SPT to the source node.
    List<GraphEdge> m_ShortestPathTree;

    //this is an indexed (by node) vector of 'parent' edges leading to nodes 
    //connected to the SPT but that have not been added to the SPT yet. This is
    //a little like the stack or queue used in BST and DST searches.
    List<GraphEdge> m_SearchFrontier;

    int m_iSource;
    int m_iTarget;

    void Search()
    {
        //create an indexed priority queue that sorts smallest to largest
        //(front to back).Note that the maximum number of elements the iPQ
        //may contain is N. This is because no node can be represented on the 
        //queue more than once.
        IndexedPriorityQueueLow pq = new IndexedPriorityQueueLow(m_FCosts, m_Graph.NumNodes());

        //put the source node on the queue
        pq.insert(m_iSource);

        //while the queue is not empty
        while (!pq.empty())
        {
            //get lowest cost node from the queue. Don't forget, the return value
            //is a *node index*, not the node itself. This node is the node not already
            //on the SPT that is the closest to the source node
            int NextClosestNode = pq.Pop();

            //move this edge from the frontier to the shortest path tree
            m_ShortestPathTree[NextClosestNode] = m_SearchFrontier[NextClosestNode];

            //if the target has been found exit
            if (NextClosestNode == m_iTarget) return;

            //now to relax the edges.
            //for each edge connected to the next closest node
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

                    pq.insert(edge.To());

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
                    pq.ChangePriority(edge.To());

                    m_SearchFrontier[edge.To()] = edge;
                }
            }
        }
    }
    public Graph_SearchAStar(SparseGraph graph, AStarHeuristic heuristic, int source, int target = -1)
    {
        m_Graph = graph;
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

        Search();
    }

    //returns the vector of edges that defines the SPT. If a target was given
    //in the constructor then this will be an SPT comprising of all the nodes
    //examined before the target was found, else it will contain all the nodes
    //in the graph.
    public List<GraphEdge> GetSPT() { return m_ShortestPathTree; }

    //returns a vector of node indexes that comprise the shortest path
    //from the source to the target. It calculates the path by working
    //backwards through the SPT from the target node.
    //supposed to be linked list
    public List<int> GetPathToTarget()
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

    //returns the total cost to the target
    public double GetCostToTarget() { return m_GCosts[m_iTarget]; }
}
