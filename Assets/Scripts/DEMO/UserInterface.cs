using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UserInterface : MonoBehaviour
{
    //UI
    public Slider sliderTimeScale;
    public Slider sliderTimeSlice;
    public TMP_Text textTimeScale;
    public TMP_Text textTimeSlice;
    public TMP_Text textPaintMode;
    public Toggle togglePaintWalls;
    public Toggle togglePaintPickups;
    public TMP_Dropdown dropdownMaps;
    public TMP_Dropdown dropdownAlgorithms;
    public TMP_Dropdown dropdownHeuristics;

    bool paintPickups = false;
    int previousPaintTile = -1;

    Game gameWorld;

    // Start is called before the first frame update
    void Start()
    {
        gameWorld = new GameObject("GameWorld").AddComponent<Game>();

        InitializeUI();
    }

    void Update()
    {
        UpdateInput();
    }

    public void UpdateInput()
    {
        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                return;
            }

            Vector3 gridWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0.5f, 0.5f, 0);
            int paintTile = Map.GetTileIndexSafe(gridWorldPos);
            if (paintTile != previousPaintTile)
            {
                previousPaintTile = paintTile;
                if (paintPickups)
                {
                    gameWorld.PaintPickups(gridWorldPos);
                }
                else
                {
                    gameWorld.PaintWalls(gridWorldPos);
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            previousPaintTile = -1;
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                return;
            }
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            gameWorld.SelectedCharacterMove(worldPos);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameWorld.SelectedCharacterFindItem();
        }
    }

    public void FindPickup()
    {
        gameWorld.SelectedCharacterFindItem();
    }

    public void TogglePaintModeWalls()
    {
        textPaintMode.text = "Paint Mode: Walls";
        paintPickups = false;
    }

    public void TogglePaintModePickups()
    {
        textPaintMode.text = "Paint Mode: Pickups";
        paintPickups = true;
    }
    public void SetTimeScale()
    {
        Clock.Instance.SetTimeScale((int)sliderTimeScale.value);
        textTimeScale.text = "Update Frequency: " + sliderTimeScale.value.ToString();
    }
    public void SetTimeSlicing()
    {
        Config.timeSliceUpdates = (uint)sliderTimeSlice.value;
        textTimeSlice.text = "Time Slices: " + sliderTimeSlice.value.ToString();
        if (Config.timeSliceUpdates == 0)
        {
            Config.timeSliceUpdates = (uint)Map.MapString().Length;
        }
    }
    public void OnMapSelect()
    {
        gameWorld.CleanUpCurrentMap();
        gameWorld.InitializeMap((uint)dropdownMaps.value);
    }
    public void SimplifiedPath()
    {
        Config.simplifiedPath = !Config.simplifiedPath;
    }
    public void MapFlipX()
    {
        Config.mapFlipX = !Config.mapFlipX;
    }
    public void MapFlipY()
    {
        Config.mapFlipY = !Config.mapFlipY;
    }
    public void ShowGraph()
    {
        Config.visualizeGraph = !Config.visualizeGraph;
        Map.VisualizeGraph(Config.visualizeGraph);
    }
    public void ShowTiles()
    {
        Config.visualizeTiles = !Config.visualizeTiles;
        Map.VisualizeTiles(Config.visualizeTiles);
    }
    public void ShowPath()
    {
        Config.visualizePath = !Config.visualizePath;
    }
    public void ShowPathfinding()
    {
        Config.visualizePathfinding = !Config.visualizePathfinding;
    }
    public void OnAlgorithmSelect()
    {
        Config.searchAlgorithm = dropdownAlgorithms.value;
    }
    public void OnHeuristicSelect()
    {
        Config.searchHeuristic = dropdownHeuristics.value;
    }
    void InitializeUI()
    {
        TextAsset[] maps = Config.GetTextMaps();
        for (int i = 0; i < maps.Length; i++)
        {
            dropdownMaps.options.Add(new TMP_Dropdown.OptionData(maps[i].name.ToString()));
        }
        dropdownMaps.RefreshShownValue();

        string[] algorithms = System.Enum.GetNames(typeof(Config.algorithms));
        foreach (string algo in algorithms)
        {
            dropdownAlgorithms.options.Add(new TMP_Dropdown.OptionData(algo));
        }
        dropdownAlgorithms.RefreshShownValue();

        string[] heuristics = System.Enum.GetNames(typeof(Config.heuristics));
        foreach (string heuristic in heuristics)
        {
            dropdownHeuristics.options.Add(new TMP_Dropdown.OptionData(heuristic));
        }
        dropdownHeuristics.RefreshShownValue();
    }

    public void RunTimeTest()
    {
        int source = Map.assignmentSourceNode;
        int target = Map.assignmentTargetNode;
        //DFS
        double time = Time.realtimeSinceStartupAsDouble;
        Graph_SearchDFS searchDFS = new Graph_SearchDFS(Map.Graph(), source, target);
        double searchTimeDFS = Time.realtimeSinceStartupAsDouble - time;
        //BFS
        time = Time.realtimeSinceStartupAsDouble;
        new Graph_SearchBFS(Map.Graph(), source, target);
        double searchTimeBFS = Time.realtimeSinceStartupAsDouble - time;
        //Dijkstra
        time = Time.realtimeSinceStartupAsDouble;
        //new Graph_SearchDijkstra(DEMO_Map.Graph(), source, target);
        new Graph_SearchDijkstra(Map.Graph(), source, new Termination_Condition_Find_Node(target));
        double searchTimeDijkstra = Time.realtimeSinceStartupAsDouble - time;
        //A-Star
        time = Time.realtimeSinceStartupAsDouble;
        Graph_SearchAStar astarSearch = new Graph_SearchAStar(Map.Graph(), new AStarHeuristic_Dijkstra(), source, target);
        double searchTimeAstarDijkstra = Time.realtimeSinceStartupAsDouble - time;
        time = Time.realtimeSinceStartupAsDouble;
        new Graph_SearchAStar(Map.Graph(), new AStarHeuristic_Euclid(), source, target);
        double searchTimeAstarEuclid = Time.realtimeSinceStartupAsDouble - time;
        time = Time.realtimeSinceStartupAsDouble;
        new Graph_SearchAStar(Map.Graph(), new AStarHeuristic_Noisy_Euclidian(), source, target);
        double searchTimeAstarNoisy_Euclidian = Time.realtimeSinceStartupAsDouble - time;
        time = Time.realtimeSinceStartupAsDouble;
        new Graph_SearchAStar(Map.Graph(), new AStarHeuristic_Manhattan(), source, target);
        double searchTimeAstarManhattan = Time.realtimeSinceStartupAsDouble - time;

        //to milliseconds
        double scale = 1000;

        Debug.Log("SEARCH TIME TEST ON MAP " + Config.GetTextMaps()[dropdownMaps.value].name + " (ms) :");
        Debug.Log("\tSearch time DFS: " + (searchTimeDFS * scale).ToString());
        Debug.Log("\tSearch time BFS: " + (searchTimeBFS * scale).ToString());
        Debug.Log("\tSearch time Dijkstra: " + (searchTimeDijkstra * scale).ToString());
        Debug.Log("\tSearch time Astar (Heuristic : Dijkstra): " + (searchTimeAstarDijkstra * scale).ToString());
        Debug.Log("\tSearch time Astar (Heuristic : Euclidian): " + (searchTimeAstarEuclid * scale).ToString());
        Debug.Log("\tSearch time Astar (Heuristic : Noisy Euclidian): " + (searchTimeAstarNoisy_Euclidian * scale).ToString());
        Debug.Log("\tSearch time Astar (Heuristic : Manhattan): " + (searchTimeAstarManhattan * scale).ToString());
    }
}
