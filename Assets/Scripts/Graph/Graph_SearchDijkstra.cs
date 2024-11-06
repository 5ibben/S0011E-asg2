using System.Collections.Generic;

public class Graph_SearchDijkstra
{
    SparseGraph m_Graph;

    //this vector contains the edges that comprise the shortest path tree -
    //a directed subtree of the graph that encapsulates the best paths from 
    //every node on the SPT to the source node.
    List<GraphEdge> m_ShortestPathTree;

    //this is indexed into by node index and holds the total cost of the best
    //path found so far to the given node. For example, m_CostToThisNode[5]
    //will hold the total cost of all the edges that comprise the best path
    //to node 5, found so far in the search (if node 5 is present and has 
    //been visited)
    List<double> m_CostToThisNode;

    //this is an indexed (by node) vector of 'parent' edges leading to nodes 
    //connected to the SPT but that have not been added to the SPT yet. This is
    //a little like the stack or queue used in BST and DST searches.
    List<GraphEdge>     m_SearchFrontier;

    int m_iSource;
    int m_iTarget;
    Graph_Serach_Termination_Condition termination_Condition;

    void Search()
    {
        //create an indexed priority queue that sorts smallest to largest
        //(front to back).Note that the maximum number of elements the iPQ
        //may contain is N. This is because no node can be represented on the 
        //queue more than once.
        IndexedPriorityQueueLow pq = new IndexedPriorityQueueLow(m_CostToThisNode, m_Graph.NumNodes());

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
            if (NextClosestNode == m_iTarget)
            {
                return;
            }
            //if (termination_Condition.isSatisfied(m_Graph, NextClosestNode))
            //{
            //    //Debug.Log("termination condition satis0fied by node: " + NextClosestNode);
            //    m_iTarget = NextClosestNode;
            //    return;
            //}


            //now to relax the edges.
            //for each edge connected to the next closest node
            foreach (GraphEdge edge in m_Graph.GetNodeEdges(NextClosestNode))
            {
                //the total cost to the node this edge points to is the cost to the
                //current node plus the cost of the edge connecting them.
                double NewCost = m_CostToThisNode[NextClosestNode] + edge.Cost();

                //if this edge has never been on the frontier make a note of the cost
                //to get to the node it points to, then add the edge to the frontier
                //and the destination node to the PQ.
                if (m_SearchFrontier[edge.To()] is null)
                {
                    m_CostToThisNode[edge.To()] = NewCost;

                    pq.insert(edge.To());

                    m_SearchFrontier[edge.To()] = edge;
                }

                //else test to see if the cost to reach the destination node via the
                //current node is cheaper than the cheapest cost found so far. If
                //this path is cheaper, we assign the new cost to the destination
                //node, update its entry in the PQ to reflect the change and add the
                //edge to the frontier
                else if ((NewCost < m_CostToThisNode[edge.To()]) && (m_ShortestPathTree[edge.To()] is null))
                //else if ((NewCost < m_CostToThisNode[edge.To()]) && (m_ShortestPathTree[edge.To()] == 0))
                {
                    m_CostToThisNode[edge.To()] = NewCost;

                    //because the cost is less than it was previously, the PQ must be
                    //re-sorted to account for this.
                    pq.ChangePriority(edge.To());

                    m_SearchFrontier[edge.To()] = edge;
                }
            }
        }
    }
    public Graph_SearchDijkstra(SparseGraph graph, int source, Graph_Serach_Termination_Condition termination)
    {
        m_Graph = graph;
        m_iSource = source;
        m_iTarget = -1;
        termination_Condition = termination;
        m_ShortestPathTree = new List<GraphEdge>(m_Graph.NumNodes());
        m_SearchFrontier = new List<GraphEdge>(m_Graph.NumNodes());
        m_CostToThisNode = new List<double>(m_Graph.NumNodes());
        for (int i = 0; i < m_Graph.NumNodes(); i++)
        {
            m_ShortestPathTree.Add(null);
            m_SearchFrontier.Add(null);
            m_CostToThisNode.Add(0);
        }
        Search();
    }

    public Graph_SearchDijkstra(SparseGraph graph, int source, int target)
    {
        m_Graph = graph;
        m_iSource = source;
        m_iTarget = target;
        m_ShortestPathTree = new List<GraphEdge>(m_Graph.NumNodes());
        m_SearchFrontier = new List<GraphEdge>(m_Graph.NumNodes());
        m_CostToThisNode = new List<double>(m_Graph.NumNodes());
        for (int i = 0; i < m_Graph.NumNodes(); i++)
        {
            m_ShortestPathTree.Add(null);
            m_SearchFrontier.Add(null);
            m_CostToThisNode.Add(0);
        }
        Search();
    }

    //returns the vector of edges that defines the SPT. If a target was given
    //in the constructor then this will be an SPT comprising of all the nodes
    //examined before the target was found, else it will contain all the nodes
    //in the graph.
    public List<GraphEdge> GetSPT(){return m_ShortestPathTree;}

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
    public double GetCostToTarget(){return m_CostToThisNode[m_iTarget];}

    //returns the total cost to the given node
    public double GetCostToNode(int nd){return m_CostToThisNode[nd];}
}
