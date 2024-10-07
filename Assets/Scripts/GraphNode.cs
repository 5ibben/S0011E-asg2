using UnityEngine;

enum NodeType
{
    invalid_node_index = -1
}

public class GraphNode : MonoBehaviour
{
    //every node has an index. A valid index is >= 0
    protected int m_iIndex;

    //constructors
    public GraphNode()
    {
        m_iIndex = (int)NodeType.invalid_node_index;
    }
    public GraphNode(int idx)
    {
        m_iIndex = idx;
    }

    public virtual void dummy() {}

    public int Index(){return m_iIndex;}
    public void SetIndex(int NewIndex) { m_iIndex = NewIndex; }
}

public class GraphEdge : MonoBehaviour
{
    //An edge connects two nodes. Valid node indices are always positive.
    int m_iFrom;
    int m_iTo;
    //the cost of traversing the edge
    double m_dCost;

    //ctors
    public GraphEdge(int from, int to, double cost)
    {
        m_dCost = cost;
        m_iFrom = from;
        m_iTo = to;
    }

    public GraphEdge(int from, int to)
    {
        m_dCost = 1.0f;
        m_iFrom = from;
        m_iTo = to;
    }

    public GraphEdge()
    {
        m_dCost = 1.0f;
        m_iFrom = (int)NodeType.invalid_node_index;
        m_iTo = (int)NodeType.invalid_node_index; ;
    }
    
    public virtual void dummy() { }

    public int From() { return m_iFrom; }
    public void SetFrom(int NewIndex) { m_iFrom = NewIndex; }
    public int To() { return m_iTo; }
    public void SetTo(int NewIndex) { m_iTo = NewIndex; }
    public double Cost() { return m_dCost; }
    public void SetCost(double NewCost) { m_dCost = NewCost; }
}
