using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{
    public GameObject floor;
    public GameObject wall;
    public GameObject pickup;
    public GameObject player;
    public GameObject spawnPoint;

    public GameObject node;
    public GameObject edge;
    public GameObject pathNode;
    public GameObject pathEdge;

    Dictionary<char, int> textMapTileCosts = new Dictionary<char, int>();
    Dictionary<char, GameObject> textMapTileObjects = new Dictionary<char, GameObject>();
    static Dictionary<char, GameObject> textMapWorldObjects = new Dictionary<char, GameObject>();

    Config() 
    {
    }

    private void Awake()
    {
        floor = Resources.Load<GameObject>("Prefabs/floor");
        wall = Resources.Load<GameObject>("Prefabs/wall");
        pickup = Resources.Load<GameObject>("Prefabs/pickup");
        player = Resources.Load<GameObject>("Prefabs/player");
        spawnPoint = Resources.Load<GameObject>("Prefabs/spawnPoint");

        node = Resources.Load<GameObject>("Prefabs/node");
        edge = Resources.Load<GameObject>("Prefabs/edge");
        pathNode = Resources.Load<GameObject>("Prefabs/pathNode");
        pathEdge = Resources.Load<GameObject>("Prefabs/pathEdge");

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

}
