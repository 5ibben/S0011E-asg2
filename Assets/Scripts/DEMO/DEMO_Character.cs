using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DEMO_Character : BaseGameEntity
{
    public GameObject pathMarker;
    List<GameObject> pathMarkers = new List<GameObject>();
    GameObject pathMarkerContainer;

    List<PathEdge> currentPath = new List<PathEdge>();

    DEMO_Game m_pWorld;
    DEMO_PathPlanner pathPlanner;

    public StateMachine<DEMO_Character> stateMachine;

    public float movementSpeed = 1.0f;
    float boundingRadius = 0.4f;
    float movementDistance = 0;
    float pathLerp = 0;
    int pathIndex = 0;
    public int currentItemSearch = 0;
    public Vector2 moveDestination;
    public bool is_moving = false;

    public float BoundingRadius()
    { 
        return boundingRadius;
    }

    public void Initialize(DEMO_Game gameWorld)
    {
        m_pWorld = gameWorld;
    }

    // Start is called before the first frame update
    void Start()
    {
        pathPlanner = new DEMO_PathPlanner(this);
        pathMarkerContainer = new GameObject("Path Markers");
    }

    // Update is called once per frame
    void Update()
    {
        if (pathIndex < currentPath.Count)
        {
            pathLerp += movementSpeed / (movementDistance+0.001f) * Time.deltaTime;
            transform.position = Vector3.Lerp(currentPath[pathIndex].Source(), currentPath[pathIndex].Destination(), pathLerp);

            while (1.0f < pathLerp)
            {
                pathLerp -= 1.0f;
                pathIndex++;
                SetPathEdge(pathIndex);
            }
        }
    }

    void SetPathEdge(int pathIndex)
    {
        if (pathIndex < currentPath.Count)
        {
            Vector3 direction = currentPath[pathIndex].Destination() - currentPath[pathIndex].Source();
            transform.up = direction.normalized;
            movementDistance = direction.magnitude;
        }
        else
        {
            is_moving = false;
        }
    }

    public void OnPathReady()
    {
        currentPath.Clear();
        currentPath = pathPlanner.GetPath();
        pathLerp = 0;
        pathIndex = 0;
        SetPathEdge(pathIndex);

        VisualizePath();
    }

    public void FindItem()
    {
        is_moving = true;
        pathPlanner.RequestPathToItem(currentItemSearch);
    }

    public void RequestMove()
    {
        is_moving = true;
        currentPath.Clear();
        pathPlanner.RequestPathToPosition(moveDestination, currentPath);
        if (0 < currentPath.Count)
        {
            pathLerp = 0;
            pathIndex = 0;
            SetPathEdge(pathIndex);
            VisualizePath();
        }
    }

    public void VisualizePath()
    {
        CleanUpMarkers();
        pathMarkers.Clear();
        if (Config.visualizePath)
        {
            for (int i = 0; i < currentPath.Count; i++)
            {
                pathMarkers.Add(Instantiate(Config.pathMarker, currentPath[i].Destination(), Quaternion.identity));
                pathMarkers[pathMarkers.Count - 1].GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
                pathMarkers[pathMarkers.Count - 1].transform.SetParent(pathMarkerContainer.transform);
            }
        }
    }

    public void CleanUpMarkers()
    {
        Destroy(pathMarkerContainer);
        pathMarkerContainer = new GameObject("Path Markers");
        pathMarkers.Clear();
    }

    public DEMO_Game GetWorld()
    {
        return m_pWorld;
    }

    public override bool HandleMessage(Telegram msg) 
    {
        return stateMachine.HandleMessage(msg);
    }

    public override void EntityStart()
    {
        Debug.Log("EntityStart()");
        SetAutomaticID();
        stateMachine = new StateMachine<DEMO_Character>(this);
        stateMachine.SetGlobalState(CharacterStateGlobal.Instance);
        stateMachine.SetCurrentState(CharacterStateIdle.Instance);
    }

    public override void EntityDestroy() 
    {
        pathPlanner.CleanUp();
        CleanUpMarkers();
    }

    public override void EntityUpdate() 
    {
        stateMachine.UpdateFSM();
    }
}
