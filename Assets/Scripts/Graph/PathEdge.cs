using UnityEngine;

public class PathEdge
{
    //positions of the source and destination nodes this edge connects
    Vector2 m_vSource;
    Vector2 m_vDestination;

    //the behavior associated with traversing this edge
    int m_iBehavior;

    int m_iDoorID;

    public PathEdge(Vector2 Source, Vector2 Destination, int Behavior, int DoorID = 0)
    {
        m_vSource = Source;
        m_vDestination = Destination;
        m_iBehavior = Behavior;
        m_iDoorID = DoorID;
    }
    public Vector2 Destination(){return m_vDestination;}
    public void SetDestination(Vector2 NewDest) { m_vDestination = NewDest; }

    public Vector2 Source(){return m_vSource;}
    public void SetSource(Vector2 NewSource) { m_vSource = NewSource; }

    public int DoorID(){return m_iDoorID;}
    public int Behavior(){return m_iBehavior;}
};
