using UnityEngine;

public class Nodetype
{
    public const int invalid_node_index = -1;
}

public class GraphNode
{
    //every node has an index. A valid index is >= 0
    protected int m_iIndex;

    //constructors
    public GraphNode()
    {
        m_iIndex = (int)Nodetype.invalid_node_index;
    }
    public GraphNode(int idx)
    {
        m_iIndex = idx;
    }

    public virtual void dummy() {}

    public int Index(){return m_iIndex;}
    public void SetIndex(int NewIndex) { m_iIndex = NewIndex; }
}

public class GraphNode_Demo : GraphNode
{
    //constructors
    public GraphNode_Demo()
    {
        m_iIndex = (int)Nodetype.invalid_node_index;
        connectedItem = null;
    }
    public GraphNode_Demo(int idx, int nodeCost, Vector2 position)
    {
        m_iIndex = idx;
        cost = nodeCost;
        pos = position;
        connectedItem = null;
    }

    public int GetCost() { return cost; }
    public void SetCost(int nodeCost) { cost = nodeCost; }
    public Vector2 GetPos() { return pos; }
    public void SetPos(Vector2 pos) { this.pos = pos; }
    public GameObject GetItem() { return connectedItem; }
    public void SetItem(GameObject go) { connectedItem = go; }

    int cost;
    Vector2 pos;
    GameObject connectedItem;
}

public class GraphEdge
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
        m_iFrom = (int)Nodetype.invalid_node_index;
        m_iTo = (int)Nodetype.invalid_node_index; ;
    }
    
    public virtual void dummy() { }

    public int From() { return m_iFrom; }
    public void SetFrom(int NewIndex) { m_iFrom = NewIndex; }
    public int To() { return m_iTo; }
    public void SetTo(int NewIndex) { m_iTo = NewIndex; }
    public double Cost() { return m_dCost; }
    public void SetCost(double NewCost) { m_dCost = NewCost; }
}

public class NavGraphEdge : GraphEdge
{
    //examples of typical flags
    public enum behaviors
    {
    normal = 0,
    swim = 1 << 0,
    crawl = 1 << 1,
    creep = 1 << 3,
    jump = 1 << 3,
    fly = 1 << 4,
    grapple = 1 << 5,
    goes_through_door = 1 << 6
    };

  int m_iFlags;

    //if this edge intersects with an object (such as a door or lift), then this is that object's ID. 
    int m_iIDofIntersectingEntity;

    public NavGraphEdge(int from, int to, double cost, int flags = 0, int id = -1):base(from, to, cost)
    {
        m_iFlags = flags;
        m_iIDofIntersectingEntity = id;
    }
    public int Flags(){return m_iFlags;}
    public void SetFlags(int flags) { m_iFlags = flags; }

    public int IDofIntersectingEntity(){return m_iIDofIntersectingEntity;}
    public void SetIDofIntersectingEntity(int id) { m_iIDofIntersectingEntity = id; }
};
