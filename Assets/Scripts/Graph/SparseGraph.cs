using System.Collections.Generic;
using UnityEngine;

public abstract class Graph
{
    public abstract int NumNodes();
}

public class SparseGraph : Graph
{
    //the nodes that comprise this graph
    List<GraphNode> m_Nodes = new List<GraphNode>();
    //a vector of adjacency edge lists. (each node index keys into the list of edges associated with that node)
    List<List<GraphEdge>> m_Edges = new List<List<GraphEdge>>();
    //is this a directed graph?
    bool m_bDigraph;
    //the index of the next node to be added
    int m_iNextNodeIndex;

    public List<GraphNode> GetNodes()
    {
        return m_Nodes;
    }
    public List<List<GraphEdge>> GetEdges()
    {
        return m_Edges;
    }
    public List<GraphEdge> GetNodeEdges(int node)
    {
        return m_Edges[node];
    }
    //returns true if an edge is not already present in the graph. Used when adding edges to make sure no duplicates are created.
    bool UniqueEdge(int from, int to)
    {
        foreach (GraphEdge edge in m_Edges[from])
        {
            if (edge.To() == to)
            {
                return false;
            }
        }
        return true;
    }

    //iterates through all the edges in the graph and removes any that point to an invalidated node
    public void CullInvalidEdges()
    {
        for (int edgeListIndex = 0; edgeListIndex < m_Edges.Count; edgeListIndex++)
        {
            for (int i = m_Edges[edgeListIndex].Count-1; i >= 0; i--)
            {
                GraphEdge myEdge = m_Edges[edgeListIndex][i];
                if (m_Nodes[myEdge.To()].Index() == (int)Nodetype.invalid_node_index || m_Nodes[myEdge.From()].Index() == (int)Nodetype.invalid_node_index)
                {
                    m_Edges[edgeListIndex].RemoveAt(i);
                }
            }
        }
    }

    public SparseGraph(bool digraph)
    {
        m_iNextNodeIndex = 0;
        m_bDigraph = digraph;
    }

    public GraphNode GetNode(int idx) { return m_Nodes[idx]; }
    public GraphEdge GetEdge(int from, int to)
    {
        Debug.Assert((from < m_Nodes.Count) && (from >= 0) && m_Nodes[from].Index() != (int)Nodetype.invalid_node_index, "<SparseGraph::GetEdge>: invalid 'from' index");
        Debug.Assert((to < m_Nodes.Count) && (to >= 0) && m_Nodes[to].Index() != (int)Nodetype.invalid_node_index, "<SparseGraph::GetEdge>: invalid 'to' index");

        foreach (GraphEdge edge in m_Edges[from])
        {
            if (edge.To() == to)
            {
                return edge;
            }
        }

        Debug.Assert(false, "<SparseGraph::GetEdge>: edge does not exist");
        return new GraphEdge();
    }
    //retrieves the next free node index
    public int GetNextFreeNodeIndex() { return m_iNextNodeIndex; }
    //adds a node to the graph and returns its index
    public int AddNode(GraphNode node)
    {
        if (node.Index() < m_Nodes.Count)
        {
            //make sure the client is not trying to add a node with the same ID as a currently active node
            Debug.Assert(m_Nodes[node.Index()].Index() == (int)Nodetype.invalid_node_index, "<SparseGraph::AddNode>: Attempting to add a node with a duplicate ID");

            m_Nodes[node.Index()] = node;

            return m_iNextNodeIndex;
        }
        else
        {
            //make sure the new node has been indexed correctly
            Debug.Assert(node.Index() == m_iNextNodeIndex, "<SparseGraph::AddNode>:invalid index");

            m_Nodes.Add(node);
            m_Edges.Add(new List<GraphEdge>());

            return m_iNextNodeIndex++;
        }
    }

    //removes a node by setting its index to invalid_node_index
    public void RemoveNode(int node)
    {
        Debug.Assert(node < (int)m_Nodes.Count, "<SparseGraph::RemoveNode>: invalid node index");

        if (m_Nodes[node].Index() == ((int)Nodetype.invalid_node_index))
        {
            return;
        }
        //set this node's index to invalid_node_index
        m_Nodes[node].SetIndex((int)Nodetype.invalid_node_index);

        //if the graph is not directed remove all edges leading to this node and then clear the edges leading from the node
        if (!m_bDigraph)
        {
            //visit each neighbour and erase any edges leading to this node
            foreach (GraphEdge edge in m_Edges[node].ToArray())
            {
                for (int neighbourEdge = m_Edges[edge.To()].Count-1; neighbourEdge >= 0; neighbourEdge--)
                {
                    if (m_Edges[edge.To()][neighbourEdge].To() == node)
                    {
                        m_Edges[edge.To()].RemoveAt(neighbourEdge);
                        break;
                    }
                }
            }

            //finally, clear this node's edges
            m_Edges[node].Clear();
        }
        //if a digraph remove the edges the slow way
        else
        {
            CullInvalidEdges();
        }
    }

    public void AddEdge(GraphEdge edge)
    {
        //first make sure the from and to nodes exist within the graph
        Debug.Assert((edge.From() < m_iNextNodeIndex) && (edge.To() < m_iNextNodeIndex), "<SparseGraph::AddEdge>: invalid node index");

        //make sure both nodes are active before adding the edge
        if ((m_Nodes[edge.To()].Index() != (int)Nodetype.invalid_node_index) && (m_Nodes[edge.From()].Index() != (int)Nodetype.invalid_node_index))
        {
            //add the edge, first making sure it is unique
            if (UniqueEdge(edge.From(), edge.To()))
            {
                m_Edges[edge.From()].Add(edge);
            }

            //if the graph is undirected we must add another connection in the opposite direction
            if (!m_bDigraph)
            {
                //check to make sure the edge is unique before adding
                if (UniqueEdge(edge.To(), edge.From()))
                {
                    GraphEdge NewEdge = new GraphEdge(edge.From(), edge.To(), edge.Cost());

                    NewEdge.SetTo(edge.From());
                    NewEdge.SetFrom(edge.To());

                    m_Edges[edge.To()].Add(NewEdge);
                }
            }
        }
    }

    //removes the edge connecting from and to from the graph (if present). If a digraph then the edge connecting the nodes in the opposite direction  will also be removed.
    public void RemoveEdge(int from, int to)
    {
        Debug.Assert((from < (int)m_Nodes.Count) && (to < (int)m_Nodes.Count), "<SparseGraph::RemoveEdge>:invalid node index");

        //remove the edge in the opposite direction
        if (!m_bDigraph)
        {
            foreach (GraphEdge edge in m_Edges[to])
            {
                if (edge.To() == from)
                {
                    m_Edges[to].Remove(edge);
                }
            }
        }
        //remove the edge
        foreach (GraphEdge edge in m_Edges[from])
        {
            if (edge.To() == to)
            {
                m_Edges[from].Remove(edge);
            }
        }
    }

    //returns the number of active + inactive nodes present in the graph
    public override int NumNodes() { return m_Nodes.Count; }
    //returns the number of active nodes present in the graph
    public int NumActiveNodes()
    {
        int count = 0;

        for (int n = 0; n < m_Nodes.Count; ++n)
        {
            if (m_Nodes[n].Index() != (int)Nodetype.invalid_node_index) ++count;
        }

        return count;
    }
    //returns the number of edges present in the graph
    public int NumEdges()
    {
        int tot = 0;

        foreach (List<GraphEdge> edges in m_Edges)
        {
            tot += edges.Count;
        }

        return tot;
    }
    //returns true if the graph is directed
    public bool isDigraph() { return m_bDigraph; }
    //returns true if the graph contains no nodes
    public bool isEmpty() { return 0 == m_Nodes.Count; }
    //returns true if a node with the given index is present in the graph
    public bool isPresent(int nd)
    {
        if ((m_Nodes[nd].Index() == (int)Nodetype.invalid_node_index) || (nd >= m_Nodes.Count))
        {
            return false;
        }
        else return true;
    }
    //clears the graph ready for new node insertions
    public void Clear() 
    { 
        m_iNextNodeIndex = 0; 
        m_Nodes.Clear(); 
        m_Edges.Clear(); 
    }
}
