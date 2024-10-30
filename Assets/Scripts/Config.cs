using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{
    public TextAsset textMap;

    public static TextAsset[] textMaps;

    public GameObject floor;
    public GameObject wall;
    public GameObject pickup;
    public GameObject player;
    public GameObject spawnPoint;

    public GameObject node;
    public GameObject edge;
    public static GameObject pathMarker;
    public static GameObject pathFinderNode;

    public static bool mapFlipX = false;
    public static bool mapFlipY = false;
    public static bool visualizeGraph = true;
    public static bool visualizeTiles = true;
    public static bool visualizePath = true;
    public static bool visualizePathfinding = true;
    public static bool simplifiedPath = true;

    public static int searchAlgorithm = (int)algorithms.ASTAR;
    public static int searchHeuristic = (int)heuristics.Euclid;
    public static uint timeSliceUpdates = 1;

    Dictionary<char, int> textMapTileCosts = new Dictionary<char, int>();
    Dictionary<char, GameObject> textMapTileObjects = new Dictionary<char, GameObject>();
    static Dictionary<char, GameObject> textMapWorldObjects = new Dictionary<char, GameObject>();

    public enum PickUps
    {
        PU0, PU1
    }
    public enum algorithms
    {
        ASTAR, Dijkstra//, DFS, BFS
    }
    public enum heuristics//ASTAR heuristic
    {
        Euclid, Euclid_Noisy, Dijkstra, Manhattan
    }

    Config() 
    {
    }

    private void Awake()
    {
        textMaps = Resources.LoadAll<TextAsset>("Maps/");
        textMap = Resources.Load<TextAsset>("Maps/Map1");

        floor = Resources.Load<GameObject>("Prefabs/floor");
        wall = Resources.Load<GameObject>("Prefabs/wall");
        pickup = Resources.Load<GameObject>("Prefabs/pickup");
        player = Resources.Load<GameObject>("Prefabs/player");
        spawnPoint = Resources.Load<GameObject>("Prefabs/spawnPoint");

        node = Resources.Load<GameObject>("Prefabs/node");
        edge = Resources.Load<GameObject>("Prefabs/edge");
        pathMarker = Resources.Load<GameObject>("Prefabs/pathMarker");
        pathFinderNode = Resources.Load<GameObject>("Prefabs/pathFinderNode");

        AddTextMapDefinitionsAsg2();
    }

    //this is a singleton
    private static Config instance = null;
    public static Config Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("Config").AddComponent<Config>();
            }
            return instance;
        }
    }

    //TODO: load definitions from file
    //text map definitions asg.2
    void AddTextMapDefinitionsAsg2()
    {
        //tile costs
        textMapTileCosts.Add('0', 1);
        textMapTileCosts.Add('X', -1);
        textMapTileCosts.Add('S', 1);
        textMapTileCosts.Add('G', 1);
        //tile objects
        textMapTileObjects.Add('0', floor);
        textMapTileObjects.Add('X', wall);
        textMapTileObjects.Add('S', floor);
        textMapTileObjects.Add('G', floor);
        //game objects
        textMapWorldObjects.Add('G', pickup);
        textMapWorldObjects.Add('S', spawnPoint);
    }

    public  int GetTextMapTileCost(char tile)
    {
        return textMapTileCosts.GetValueOrDefault(tile, -1);
    }
    public GameObject GetTextMapTileObject(char tile)
    {
        return textMapTileObjects.GetValueOrDefault(tile, null);
    }
    public static GameObject GetTextMapWorldObject(char tile)
    {
        return textMapWorldObjects.GetValueOrDefault(tile, null);
    }
    public static TextAsset[] GetTextMaps()
    {
        return textMaps;
    }
    public TextAsset GetTextMap(uint i)
    {
        if (textMaps.Length <= i)
        {
            i = 0;
        }
        return textMaps[i];
    }
}
