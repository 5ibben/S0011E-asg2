using System.Collections.Generic;

public class Graph_SearchBFS
{
    enum NodeStatus { visited, unvisited, no_parent_assigned };

    //a reference to the graph to be searched
    SparseGraph m_Graph;
    //this records the indexes of all the nodes that are visited as the search progresses
    List<int> m_Visited;

    //this holds the route taken to the target.
    List<int> m_Route;

    //As the search progresses, this will hold all the edges the algorithm has
    //examined. THIS IS NOT NECESSARY FOR THE SEARCH, IT IS HERE PURELY
    //TO PROVIDE THE USER WITH SOME VISUAL FEEDBACK
    List<GraphEdge> m_SpanningTree = new List<GraphEdge>();

    //the source and target node indices
    int m_iSource;
    int m_iTarget;

    //true if a path to the target has been found
    bool m_bFound;
    //this method performs the DFS search
    bool Search()
    {
        //create a queue of edges
        Queue<GraphEdge> queue = new Queue<GraphEdge>();
        //create a dummy edge and put on the stack
        GraphEdge Dummy = new GraphEdge(m_iSource, m_iSource, 0);
        queue.Enqueue(Dummy);

        //mark the source node as visited
        m_Visited[m_iSource] = (int)NodeStatus.visited;
        //while there are edges on the stack keep searching
        while (0 < queue.Count)
        {
            //grab the next edge and remove it from the stack
            GraphEdge Next = queue.Dequeue();
            //make a note of the parent of the node this edge points to
            m_Route[Next.To()] = Next.From();
            //put it on the tree. (making sure the dummy edge is not placed on the tree)
            if (Next != Dummy)
            {
                m_SpanningTree.Add(Next);
            }

            //if the target has been found the method can return success
            if (Next.To() == m_iTarget)
            {
                return true;
            }

            //push the edges leading from the node this edge points to onto the stack (provided the edge does not point to a previously visited node)
            foreach (GraphEdge edge in m_Graph.GetNodeEdges(Next.To()))
            {
                if (m_Visited[edge.To()] == (int)NodeStatus.unvisited)
                {
                    queue.Enqueue(edge);
                    //and mark it visited
                    m_Visited[edge.To()] = (int)NodeStatus.visited;
                }
            }
        }
        //no path to target
        return false;
    }

    public Graph_SearchBFS(SparseGraph graph, int source, int target = -1)
    {
        m_Graph = graph;
        m_iSource = source;
        m_iTarget = target;
        m_bFound = false;
        m_Visited = new List<int>(m_Graph.NumNodes());
        for (int i = 0; i < m_Graph.NumNodes(); i++)
        {
            m_Visited.Add((int)NodeStatus.unvisited);
        }
        m_Route = new List<int>(m_Graph.NumNodes());
        for (int i = 0; i < m_Graph.NumNodes(); i++)
        {
            m_Route.Add((int)NodeStatus.no_parent_assigned);
        }
        m_bFound = Search();
    }
    //returns true if the target node has been located
    public bool Found() { return m_bFound; }

    //returns a vector containing pointers to all the edges the search has examined
    public List<GraphEdge> GetSearchTree(){return m_SpanningTree;}

    //returns a vector of node indexes that comprise the shortest path from the source to the target
    public List<int> GetPathToTarget()
    {
        List<int> path = new List<int>();

        //just return an empty path if no path to target found or if
        //no target has been specified
        if (!m_bFound || m_iTarget < 0) return path;

        int nd = m_iTarget;

        path.Add(nd);

        while (nd != m_iSource)
        {
            nd = m_Route[nd];

            path.Add(nd);
        }

        return path;
    }
}
