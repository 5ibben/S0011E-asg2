using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Graph_Serach_Termination_Condition : MonoBehaviour
{
    public abstract bool isSatisfied(SparseGraph graph, int CurrentNodeIdx);
}

public class Find_Object_Termination : Graph_Serach_Termination_Condition
{
    DEMO_PickUp targetItem;
    public Find_Object_Termination(DEMO_PickUp objectToFind)
    {
        targetItem = objectToFind;
    }
    public override bool isSatisfied(SparseGraph graph, int CurrentNodeIdx)
    {
        GameObject item = ((GraphNode_Demo)(graph.GetNode(CurrentNodeIdx))).GetItem();
        if (item != null)
        {
            if (targetItem.GetType() == item.GetComponent<DEMO_PickUp>().GetType())
            {
                return true;
            }
        }
        return false;
    }
}

public class DEMO_PathPlanner : MonoBehaviour
{
    //for legibility
    const int no_closest_node_found = -1;
    //A pointer to the owner of this class
    DEMO_Character m_pOwner;
    //a local reference to the navgraph
    SparseGraph m_NavGraph;
    //this is the position the bot wishes to plan a path to reach
    Vector2 m_vDestinationPos;
    //returns the index of the closest visible and unobstructed graph node to
    //the given position. If none is found it returns the enumerated value
    //"no_closest_node_found"
    int GetClosestNodeToPosition(Vector2 pos) 
    {
        //double ClosestSoFar = double.MaxValue;
        //int ClosestNode = no_closest_node_found;

        //when the cell space is queried this the the range searched for neighboring
        //graph nodes. This value is inversely proportional to the density of a 
        //navigation graph (less dense = bigger values)
        //double range = DEMO_Map.GetCellSpaceNeighborhoodRange();

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

        ////calculate the graph nodes that are neighboring this position
        //m_pOwner->GetWorld()->GetMap()->GetCellSpace()->CalculateNeighbors(pos, range);

        ////iterate through the neighbors and sum up all the position vectors
        //for (NodeType* pN = m_pOwner->GetWorld()->GetMap()->GetCellSpace()->begin(); !m_pOwner->GetWorld()->GetMap()->GetCellSpace()->end();  pN = m_pOwner->GetWorld()->GetMap()->GetCellSpace()->next())
        //{
        //    //if the path between this node and pos is unobstructed calculate the
        //    //distance
        //    if (m_pOwner->canWalkBetween(pos, pN->Pos()))
        //    {
        //        double dist = Vec2DDistanceSq(pos, pN->Pos());

        //        //keep a record of the closest so far
        //        if (dist < ClosestSoFar)
        //        {
        //            ClosestSoFar = dist;
        //            ClosestNode = pN->Index();
        //        }
        //    }
        //}

        //return ClosestNode;
    }
    public DEMO_PathPlanner(DEMO_Character owner)
    {
        m_pOwner = owner;
    }

    public List<Vector3> SmoothPathQuick(List<Vector3> path)
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


    //finds the least cost path between the agent's position and the target
    //position. Fills path with a list of waypoints if the search is successful
    //and returns true. Returns false if unsuccessful
    public bool CreatePathToPosition(Vector2 TargetPos, List<Vector3> path, bool smoothPath = false)
    {
        //make a note of the target position
        m_vDestinationPos = TargetPos;
        //if the target is unobstructed from the bot's position, a path does not need
        //to be calculated, and the bot can ARRIVE directly at the destination.
        //isPathObstructed is a method that takes a start
        //position, a target position, and an entity radius and determines if an
        //agent of that size is able to move unobstructed between the two positions.
        //It is used here to determine if the bot can move directly to the target
        //location without the need for planning a path.

        if (!DEMO_Map.DoWallsIntersectCircle(m_pOwner.transform.position, TargetPos, m_pOwner.BoundingRadius()))
        {
            //add self to path
            path.Add(m_pOwner.transform.position);
            //if (m_pOwner.transform.position != (Vector3)TargetPos)
            //{
            //}
            path.Add(TargetPos);
            return true;
        }

        //GetClosestNodeToPosition is a method that queries the navigation graph
        //nodes (via the cell-space partition) to determine the closest unobstructed
        //node to the given position vector. It is used here to find the closest
        //unobstructed node to the bot’s current location.
        int ClosestNodeToBot = GetClosestNodeToPosition(m_pOwner.transform.position);

        //if no visible node found return failure. This will occur if the
        //navgraph is badly designed or if the bot has managed to get itself
        //*inside* the geometry (surrounded by walls) or an obstacle.
        if (ClosestNodeToBot == no_closest_node_found)
        {
            return false;
        }

        //find the closest visible unobstructed node to the target position
        int ClosestNodeToTarget = GetClosestNodeToPosition(TargetPos);
        //return failure if there is a problem locating a visible node from
        //the target.
        //This sort of thing occurs much more frequently than the above. For
        //example, if the user clicks inside an area bounded by walls or inside an
        //object.
        if (ClosestNodeToTarget == no_closest_node_found)
        {
            return false;
        }
        //create an instance of the A* search class to search for a path between the
        //closest node to the bot and the closest node to the target position. This
        //A* search will utilize the Euclidean straight line heuristic
        Debug.Log("m_pOwner.transform.position: " + m_pOwner.transform.position + " to: " + TargetPos);
        Debug.Log("ASTAR search. from: "+ClosestNodeToBot + " to: " + ClosestNodeToTarget);
        Graph_SearchAStar search = new Graph_SearchAStar(DEMO_Map.Graph(), new AStarHeuristic_Euclid(), ClosestNodeToBot, ClosestNodeToTarget);

        //grab the path
        List<int> PathOfNodeIndices = search.GetPathToTarget();

        //if the search is successful convert the node indices into position vectors
        if (0 < PathOfNodeIndices.Count)
        {
            //add self to path
            if (m_pOwner.transform.position != (Vector3)((GraphNode_Demo)DEMO_Map.Graph().GetNode(PathOfNodeIndices[PathOfNodeIndices.Count-1])).GetPos())
            {
                path.Add(m_pOwner.transform.position);
            }
            for (int i = PathOfNodeIndices.Count-1; i >= 0; i--)
            {
                path.Add(((GraphNode_Demo)DEMO_Map.Graph().GetNode(PathOfNodeIndices[i])).GetPos());

            }
            //remember to add the target position to the end of the path
            if (!DEMO_Map.DoWallsIntersectCircle(TargetPos, TargetPos, m_pOwner.BoundingRadius()))
            {
                path.Add(TargetPos);
            }

            return true;
        }
        else
        {
            //no path found by the search
            return false;
        }
    }

    //finds the least cost path to an instance of ItemType. Fills path with a
    //list of waypoints if the search is successful and returns true. Returns
    //false if unsuccessful
    public bool RequestPathToItem(DEMO_PickUp ItemType, List<Vector3> path)
    {
        //find the closest visible node to the bots position
        int ClosestNodeToBot = GetClosestNodeToPosition(m_pOwner.transform.position);

        //remove the destination node from the list and return false if no visible
        //node found. This will occur if the navgraph is badly designed or if the bot
        //has managed to get itself *inside* the geometry (surrounded by walls),
        //or an obstacle
        if (ClosestNodeToBot == no_closest_node_found)
        {
            return false;
        }

        //create an instance of the search algorithm
        Graph_SearchDijkstra search = new Graph_SearchDijkstra(DEMO_Map.Graph(), ClosestNodeToBot, new Find_Object_Termination(ItemType));

        //grab the path
        List<int> PathOfNodeIndices = search.GetPathToTarget();

        //if the search is successful convert the node indices into position vectors
        if (0 < PathOfNodeIndices.Count)
        {
            //add self to path
            if (m_pOwner.transform.position != (Vector3)((GraphNode_Demo)DEMO_Map.Graph().GetNode(PathOfNodeIndices[PathOfNodeIndices.Count - 1])).GetPos())
            {
                path.Add(m_pOwner.transform.position);
            }
            for (int i = PathOfNodeIndices.Count - 1; i >= 0; i--)
            {
                path.Add(((GraphNode_Demo)DEMO_Map.Graph().GetNode(PathOfNodeIndices[i])).GetPos());
            }

            return true;
        }
        else
        {
            //no path found by the search
            return false;
        }
    }
}
