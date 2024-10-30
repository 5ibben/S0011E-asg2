using System.Collections.Generic;
using UnityEngine;

public abstract class Graph_Serach_Termination_Condition
{
    public abstract bool isSatisfied(SparseGraph graph, int CurrentNodeIdx);
}

public class Termination_Condition_Find_Node : Graph_Serach_Termination_Condition
{
    int targetNode;
    public Termination_Condition_Find_Node(int node)
    {
        targetNode = node;
    }
    public override bool isSatisfied(SparseGraph graph, int CurrentNodeIdx)
    {
        if (targetNode == CurrentNodeIdx)
        {
            return true;
        }
        return false;
    }
}

public class Termination_Condition_Find_Object : Graph_Serach_Termination_Condition
{
    int targetItem;
    public Termination_Condition_Find_Object(int objectToFind)
    {
        targetItem = objectToFind;
    }
    public override bool isSatisfied(SparseGraph graph, int CurrentNodeIdx)
    {
        GameObject item = ((GraphNode_Demo)(graph.GetNode(CurrentNodeIdx))).GetItem();
        if (item != null)
        {
            if (targetItem == item.GetComponent<DEMO_PickUp>().ItemType())
            {
                return true;
            }
        }
        return false;
    }
}

public class DEMO_PathPlanner
{
    //for legibility
    const int no_closest_node_found = -1;
    //A pointer to the owner of this class
    DEMO_Character m_pOwner;
    //a local reference to the navgraph
    SparseGraph m_NavGraph;
    //a pointer to an instance of the current graph search algorithm.
    Graph_SearchTimeSliced m_pCurrentSearch;
    //this is the position the bot wishes to plan a path to reach
    Vector2 m_vDestinationPos;

    //the status of the current search
    int currentStatus;

    public int Status() { return currentStatus; }

    //returns the index of the closest visible and unobstructed graph node to the given position.
    //THIS IS A SIMPLIFIED VERSION THAT ONLY WORKS WITH GRID GRAPHS
    int GetClosestNodeToPosition(Vector2 pos) 
    {
        int ClosestNode = DEMO_Map.GetTileIndex(pos + new Vector2(0.5f, 0.5f));


        if (0 < ClosestNode && ClosestNode < DEMO_Map.MapString().Length)
        {
            if (DEMO_Map.Graph().GetNode(ClosestNode).Index() != Nodetype.invalid_node_index)
            {
                //Debug.Log("returnbing closest node: " + ClosestNode);
                return ClosestNode;
            }
        }
        return no_closest_node_found;
    }
    public DEMO_PathPlanner(DEMO_Character owner)
    {
        m_pOwner = owner;
    }

    public List<Vector3> SmoothPointPath(List<Vector3> path)
    {
        List<Vector3> smoothPath = new List<Vector3>();

        //add first point
        smoothPath.Add(path[0]);

        int node0 = 0;
        int node1 = 2;
        while (node1 < path.Count)
        {
            Vector2 from = path[node0];
            Vector2 to = path[node1];
            if (DEMO_Map.DoWallsIntersectCircle(from, to, m_pOwner.BoundingRadius()))
            {
                smoothPath.Add(path[node1 - 1]);
                node0 = node1 - 1;
                node1 = node0 + 2;
            }
            else
            {
                node1++;
            }
        }

        //add last point
        smoothPath.Add(path[path.Count-1]);
        return smoothPath;
    }

    //smooths a path by removing extraneous edges
    public List<PathEdge> SimplifyPathEdges(List<PathEdge> path)
    {
        List<PathEdge> smoothPath = new List<PathEdge>();

        int edge0 = 0;
        int edge1 = 1;

        while (edge1 < path.Count)
        {
            if (DEMO_Map.DoWallsIntersectCircle(path[edge0].Source(), path[edge1].Destination(), m_pOwner.BoundingRadius()))
            {
                smoothPath.Add(path[edge0]);
                edge0 = edge1;
            }
            else
            {
                path[edge0].SetDestination(path[edge1].Destination());
            }
            edge1++;
        }
        //add last edge
        smoothPath.Add(path[edge0]);
        return smoothPath;
    }

    //called on every new search request.
    void GetReadyForNewSearch()
    {
        CleanUp();
    }

    public void CleanUp()
    {
        if (m_pCurrentSearch is null)
        {
            return;
        }
        //unregister any existing search with the path manager
        m_pOwner.GetWorld().GetPathManager().UnRegister(this);
        m_pCurrentSearch.CleanUP();
        m_pCurrentSearch = null;
    }

    //finds the least cost path between the agent's position and the target position
    public bool RequestPathToPosition(Vector2 TargetPos, List<PathEdge> path)
    {
        //check target bounds
        if (!DEMO_Map.CheckBounds(TargetPos + new Vector2(0.5f,0.5f)))
        {
            return false;
        }
        GetReadyForNewSearch();

        //make a note of the target position
        m_vDestinationPos = TargetPos;

        //if the target is unobstructed from the bot's position, a path does not need to be calculated, and the bot can ARRIVE directly at the destination.
        if (!DEMO_Map.DoWallsIntersectCircle(m_pOwner.transform.position, TargetPos, m_pOwner.BoundingRadius()))
        {
            path.Add(new PathEdge(m_pOwner.transform.position, TargetPos, 0, 0));
            return true;
        }

        //Find the closest unobstructed node to the bot’s current location.
        int ClosestNodeToBot = GetClosestNodeToPosition(m_pOwner.transform.position);

        //if no visible node found return failure.
        if (ClosestNodeToBot == no_closest_node_found)
        {
            return false;
        }

        //find the closest visible unobstructed node to the target position
        int ClosestNodeToTarget = GetClosestNodeToPosition(TargetPos);

        //return failure if there is a problem locating a visible node from the target.
        if (ClosestNodeToTarget == no_closest_node_found)
        {
            return false;
        }
        
        //crate a new search
        if (Config.searchAlgorithm == (int)Config.algorithms.ASTAR)
        {
            Debug.Log("search using ASTAR");
            AStarHeuristic heuristic = new AStarHeuristic_Euclid();
            if (Config.searchHeuristic == (int)Config.heuristics.Euclid)
            {
                Debug.Log("\tEuclid");
            }
            else if (Config.searchHeuristic == (int)Config.heuristics.Euclid_Noisy)
            {
                Debug.Log("\tEuclid_Noisy");
                heuristic = new AStarHeuristic_Noisy_Euclidian();
            }
            else if(Config.searchHeuristic == (int)Config.heuristics.Dijkstra)
            {
                Debug.Log("\tDijkstra");
                heuristic = new AStarHeuristic_Dijkstra();
            }
            else if (Config.searchHeuristic == (int)Config.heuristics.Manhattan)
            {
                Debug.Log("\tManhattan");
                heuristic = new AStarHeuristic_Manhattan();
            }
            m_pCurrentSearch = new Graph_SearchAStar_TS(DEMO_Map.Graph(), heuristic, ClosestNodeToBot, ClosestNodeToTarget);
        }
        else if(Config.searchAlgorithm == (int)Config.algorithms.Dijkstra)
        {
            Debug.Log("search using dijkstra");
            m_pCurrentSearch = new Graph_SearchDijkstra_TS(DEMO_Map.Graph(), ClosestNodeToBot, new Termination_Condition_Find_Node(ClosestNodeToTarget));
        }

        //and register the search with the path manager
        m_pOwner.GetWorld().GetPathManager().Register(this);

        return true;
    }

    //finds the least cost path to an instance of ItemType
    public bool RequestPathToItem(int ItemType)
    {
        //clear the waypoint list and delete any active search
        GetReadyForNewSearch();

        //find the closest visible node to the bots position
        int ClosestNodeToBot = GetClosestNodeToPosition(m_pOwner.transform.position);

        //remove the destination node from the list and return false if no visible node found
        if (ClosestNodeToBot == no_closest_node_found)
        {
            return false;
        }

        //create an instance of the search algorithm
        m_pCurrentSearch = new Graph_SearchDijkstra_TS(DEMO_Map.Graph(), ClosestNodeToBot, new Termination_Condition_Find_Object(ItemType));

        //and register the search with the path manager
        m_pOwner.GetWorld().GetPathManager().Register(this);

        return true;
    }

    //called by an agent after it has been notified that a search has terminated successfully. The method extracts the path from m_pCurrentSearch, adds
    //additional edges appropriate to the search type and returns it as a list of PathEdges.
    public List<PathEdge> GetPath()
    {
        Debug.Assert(!(m_pCurrentSearch is null), "<Raven_PathPlanner::GetPathAsNodes>: no current search");

        List<PathEdge> path = m_pCurrentSearch.GetPathAsPathEdges();

        int closest = GetClosestNodeToPosition(m_pOwner.transform.position);
        if (0.001f < (GetNodePosition(closest) - (Vector2)m_pOwner.transform.position).magnitude)
        {
            path.Add(new PathEdge(m_pOwner.transform.position, GetNodePosition(closest), 0, -1));
        }

        path.Reverse();

        //add edge to destination
        if (m_pCurrentSearch.GetType() == Graph_SearchTimeSliced.SearchType.AStar)
        {
            if (!(DEMO_Map.DoWallsIntersectCircle(path[path.Count - 1].Destination(), m_vDestinationPos, m_pOwner.BoundingRadius())))
            {
                path.Add(new PathEdge(path[path.Count-1].Destination(), m_vDestinationPos, 0, -1));
            }
        }

        //smooth paths if required
        if (Config.simplifiedPath)
        {
            path = SimplifyPathEdges(path);
        }

        return path;
    }

    //the path manager calls this to iterate once though the search cycle of the currently assigned search algorithm.
    public int CycleOnce()
    {
        Debug.Assert(!(m_pCurrentSearch is null), "<DEMO_PathPlanner::CycleOnce>: No search object instantiated");

        int result = m_pCurrentSearch.CycleOnce();
        currentStatus = result;

        //let the bot know of the failure to find a path
        if (result == (int)search_status.target_not_found)
        {
            MessageDispatcher.DispatchMessage(MessageDispatcher.SEND_MSG_IMMEDIATELY, MessageDispatcher.SENDER_ID_IRRELEVANT, m_pOwner.ID(), (int)messages.Msg_NoPathAvailable);
        }

        //let the bot know a path has been found
        else if (result == (int)search_status.target_found)
        {
            MessageDispatcher.DispatchMessage(MessageDispatcher.SEND_MSG_IMMEDIATELY, MessageDispatcher.SENDER_ID_IRRELEVANT, m_pOwner.ID(), (int)messages.Msg_PathReady);
        }

        return result;
    }

    public Vector2 GetDestination(){return m_vDestinationPos;}
    public void SetDestination(Vector2 NewPos) { m_vDestinationPos = NewPos; }

    public Vector2 GetNodePosition(int idx)
    {
        return ((GraphNode_Demo)DEMO_Map.Graph().GetNode(idx)).GetPos();
    }
}
