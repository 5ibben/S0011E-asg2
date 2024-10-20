using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DEMO_Map : MonoBehaviour
{
    DEMO_Map() { }
    //this is a singleton
    private static DEMO_Map instance = null;
    public static DEMO_Map Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject().AddComponent<DEMO_Map>();
            }
            return instance;
        }
    }

    static GameObject tileObjectContainer;
    static GameObject worldObjectContainer;
    static GameObject nodeObjectContainer;
    static GameObject edgeObjectContainer;

    static bool include_diagonals = true;
    static bool cullCornerCutters = true;

    static string mapstring = new string("");
    static int mapWidth = 0;
    static int mapHeight = 0;
    static SparseGraph graph;

    static List<GameObject> tileObjects = new List<GameObject>();
    static List<GameObject> worldObjects = new List<GameObject>();
    static List<Vector2> spawnPoints = new List<Vector2>();

    static bool visualizeGraph = false;
    static List<GameObject> nodeObjects = new List<GameObject>();
    static List<List<GameObject>> edgeObjects = new List<List<GameObject>>();

    //the size of the search radius the cellspace partition uses when looking for 
    //neighbors 
    static double m_dCellSpaceNeighborhoodRange;

    public static double GetCellSpaceNeighborhoodRange(){return m_dCellSpaceNeighborhoodRange;}

    public static SparseGraph Graph() { return graph; }
    public static int MapWidth() { return mapWidth; }
    public static int MapHeight() { return mapHeight; }
    public static string MapString() { return mapstring; }

    static string  FormatMap(TextAsset map, bool flipX, bool flipY)
    {
        //calculate map dimensions
        string mapstring = map.ToString();
        int width = 0;
        while (mapstring[width] != '\n')
        {
            width++;
        }
        mapWidth = width-1;//-1 for newline character
        mapHeight = mapstring.Length / width;

        //remove junk
        mapstring = mapstring.Replace("\n", "");
        mapstring = mapstring.Replace("\r", "");

        //set orientation
        if (flipX)
        {
            mapstring = mapFlipXY(mapstring);
            if (!flipY)
            {
                mapstring = mapFlipY(mapstring);
            }
        }
        else if (flipY)
        {
            mapstring = mapFlipY(mapstring);
        }

        Debug.Log("mapWidth: " + mapWidth);
        Debug.Log("mapHeight: " + mapHeight);
        Debug.Log("mapStringLength: " + mapstring.Length);

        return mapstring;
    }

    static void GenerateNodeEdges(int nodeIndex)
    {
        int nodeCost = ((GraphNode_Demo)graph.GetNode(nodeIndex)).GetCost();

        List<int> neighbourNodes = GetNodeNeighbourhood(nodeIndex);
        for (int i = 0; i < neighbourNodes.Count; i++)
        {
            int neighbourNode = neighbourNodes[i];
            //only valid nodes
            if (0 <= neighbourNode && graph.GetNode(neighbourNode).Index() != (int)Nodetype.invalid_node_index)
            {
                //skip self
                if (neighbourNode != nodeIndex)
                {
                    int neighbourCost = ((GraphNode_Demo)graph.GetNode(neighbourNode)).GetCost();
                    //perpendiculars
                    if (i % 2 == 1)
                    {
                        double edgeCost = ((nodeCost + neighbourCost) / 2);
                        graph.AddEdge(new GraphEdge(nodeIndex, neighbourNode, edgeCost));
                    }
                    //diagonals
                    else if (include_diagonals)
                    {
                        double edgeCost = ((nodeCost + neighbourCost) / 2) * 1.4;

                        if (cullCornerCutters)
                        {
                            //figure out influencing neighbours
                            int horizontalNeighbour = neighbourNode - 1;
                            int verticalNeighbour = nodeIndex + 1;
                            if (i % 6 == 0)
                            {
                                horizontalNeighbour = neighbourNode + 1;
                                verticalNeighbour = nodeIndex - 1;
                            }
                            if (graph.GetNode(horizontalNeighbour).Index() != (int)Nodetype.invalid_node_index && graph.GetNode(verticalNeighbour).Index() != (int)Nodetype.invalid_node_index)
                            {
                                graph.AddEdge(new GraphEdge(nodeIndex, neighbourNode, edgeCost));
                            }
                        }
                        else
                        {
                            graph.AddEdge(new GraphEdge(nodeIndex, neighbourNode, edgeCost));
                        }
                    }
                }
            }
        }
    }

    static string mapFlipXY(string mapString)
    {
        char[] flippedArray = new char[mapString.Length];
        int flippedIndex = 0;
        for (int i = mapString.Length-1; i >= 0; i--)
        {
            flippedArray[flippedIndex++] = mapString[i];
        }
        return new string(flippedArray);
    }

    static string mapFlipY(string mapString)
    {
        string flippedString = new string("");
        for (int i = mapHeight - 1; i >= 0; i--)
        {
            flippedString += mapString.Substring(i * mapWidth, mapWidth);
        }
        return flippedString;
    }

    public static int GetTileIndexSafe(Vector2 tileCoords)
    {
        //check bounds
        if (0 <= tileCoords.x && tileCoords.x < mapWidth && 0 <= tileCoords.y && tileCoords.y < mapHeight)
        {
            return ((int)tileCoords.y * mapWidth + (int)tileCoords.x);
        }
        return -1;
    }

    public static int GetTileIndex(Vector2 tileCoords)
    {
        return ((int)tileCoords.y * mapWidth + (int)tileCoords.x);
    }

    public static Vector2 GetTileCoords(int tileIndex)
    {
        return new Vector2(tileIndex % (mapWidth), tileIndex / (mapWidth));
    }

    public static void LoadGridMap(TextAsset map, bool flipX, bool flipY, bool visualize_Graph)
    {
        visualizeGraph = visualize_Graph;
        mapstring = FormatMap(map, flipX, flipY);

        nodeObjectContainer = new GameObject("nodes");
        edgeObjectContainer = new GameObject("edges");
        tileObjectContainer = new GameObject("tiles");
        worldObjectContainer = new GameObject("world objects");

        LoadGridGraph();
        LoadGridObjects();
        VisualizeGraph();
    }

    public static List<Vector2> GetSpawnPoints()
    {
        return spawnPoints;
    }

    public static GameObject GetWorldObject(Vector2 coord)
    {
        return ((GraphNode_Demo)graph.GetNode(GetTileIndex(coord))).GetItem();
    }

    public static GameObject GetWorldObject(int tileIndex)
    {
        return ((GraphNode_Demo)graph.GetNode(tileIndex)).GetItem();
    }

    public static void RemoveWorldObject(Vector2 coord)
    {
        RemoveWorldObject(GetTileIndex(coord));
    }

    public static void RemoveWorldObject(int tileIndex)
    {
        GameObject currentObject = ((GraphNode_Demo)graph.GetNode(tileIndex)).GetItem();
        if (!(currentObject is null))
        {
            Destroy(currentObject);
        }
    }

    public static void AddWorldObject(GameObject worldObject, Vector2 coord)
    {
        AddWorldObject(worldObject, GetTileIndex(coord));
    }

    public static void AddWorldObject(GameObject worldObject, int tileIndex)
    {
        if (!(worldObject is null))
        {
            //remove previous object
            RemoveWorldObject(tileIndex);
            //Add new object
            GameObject go = Instantiate(worldObject, GetTileCoords(tileIndex), Quaternion.identity);
            worldObjects.Add(go);
            go.transform.SetParent(worldObjectContainer.transform);
            ((GraphNode_Demo)graph.GetNode(tileIndex)).SetItem(go);
        }
    }

    public static GameObject GetTileObject(Vector2 coord)
    {
        return tileObjects[GetTileIndex(coord)];
    }

    public static GameObject GetTileObject(int tileIndex)
    {
        return tileObjects[tileIndex];
    }

    public static void AddTileObject(char tile, Vector3 tileCoords)
    {
        AddTileObject(tile, GetTileIndex(tileCoords));
    }

    public static void AddTileObject(char tile, int tileIndex)
    {
        GameObject tileObject = Config.Instance.GetTextMapTileObject(tile);
        if (!(tileObject is null))
        {
            Destroy(tileObjects[tileIndex]);
            tileObjects[tileIndex] = (Instantiate(tileObject, GetTileCoords(tileIndex), Quaternion.identity));
            int nodeCost = Config.Instance.GetTextMapTileCost(tile);
            if (nodeCost < 0)
            {
                //remove node
                graph.RemoveNode(tileIndex);
                //recalculate edges of neigbhourhood
                foreach (int node in GetNodeNeighbourhood(tileIndex))
                {
                    graph.GetNodeEdges(node).Clear();
                }
                foreach (int node in GetNodeNeighbourhood(tileIndex))
                {
                    GenerateNodeEdges(node);
                }

                //visuals
                nodeObjects[tileIndex].SetActive(false);
                VisualizeNodeEdges(tileIndex);
            }
            else
            {
                if (graph.GetNode(tileIndex).Index() == (int)Nodetype.invalid_node_index)
                {
                    graph.AddNode(new GraphNode_Demo(tileIndex, nodeCost, GetTileCoords(tileIndex)));
                    foreach (int node in GetNodeNeighbourhood(tileIndex))
                    {
                        GenerateNodeEdges(node);
                    }
                    //visuals
                    nodeObjects[tileIndex].SetActive(true);
                    VisualizeNodeEdges(tileIndex);
                }
            }
        }
    }

    public static void LoadGridObjects()
    {
        for (int tileIndex = 0; tileIndex < mapstring.Length; tileIndex++)
        {
            GameObject tileObject = Config.Instance.GetTextMapTileObject(mapstring[tileIndex]);
            GameObject worldObject = Config.GetTextMapWorldObject(mapstring[tileIndex]);
            if (!(tileObject is null))
            {
                GameObject go = Instantiate(tileObject, GetTileCoords(tileIndex), Quaternion.identity);
                tileObjects.Add(go);
                go.transform.SetParent(tileObjectContainer.transform);
            }
            if (!(worldObject is null))
            {
                Debug.Log("worldObject.name: " + worldObject.name);
                if (worldObject.name == "spawnPoint")
                {
                    spawnPoints.Add(GetTileCoords(tileIndex));
                }
                else
                {
                    GameObject go = Instantiate(worldObject, GetTileCoords(tileIndex), Quaternion.identity);
                    worldObjects.Add(go);
                    go.transform.SetParent(worldObjectContainer.transform);
                    ((GraphNode_Demo)graph.GetNode(tileIndex)).SetItem(go);
                }
            }
        }
    }

    public static void LoadGridGraph()
    {
        graph = new SparseGraph(false);

        //add nodes
        for (int nodeIndex = 0; nodeIndex < mapstring.Length; nodeIndex++)
        {
            int nodeCost = Config.Instance.GetTextMapTileCost(mapstring[nodeIndex]);
            int y = nodeIndex / (mapWidth);
            int x = nodeIndex % (mapWidth);
            graph.AddNode(new GraphNode_Demo(nodeIndex, nodeCost, new Vector2(x, y)));
        }

        //remove unwalkable nodes
        for (int nodeIndex = 0; nodeIndex < mapstring.Length; nodeIndex++)
        {
            int nodeCost = Config.Instance.GetTextMapTileCost(mapstring[nodeIndex]);
            int y = nodeIndex / (mapWidth);
            int x = nodeIndex % (mapWidth);
            if (nodeCost < 0)
            {
                graph.RemoveNode(nodeIndex);
            }
        }

        //add edges
        for (int nodeIndex = 0; nodeIndex < mapstring.Length; nodeIndex++)
        {
            GenerateNodeEdges(nodeIndex);
        }
    }

    static void VisualizeNodeEdges(int nodeIndex)
    {
        foreach (int neighbour in GetNodeNeighbourhood(nodeIndex))
        {
            if (0 <= neighbour)
            {
                foreach (GameObject edgeObject in edgeObjects[neighbour])
                {
                    Destroy(edgeObject);
                }
                edgeObjects[neighbour].Clear();

                List<GraphNode> nodes = graph.GetNodes();
                List<GraphEdge> nodeEdges = graph.GetNodeEdges(neighbour);

                foreach (GraphEdge edge in nodeEdges)
                {
                    GraphNode_Demo fromNode = (GraphNode_Demo)nodes[edge.From()];
                    GraphNode_Demo toNode = (GraphNode_Demo)nodes[edge.To()];

                    GameObject go = Instantiate(Config.Instance.edge);
                    go.GetComponent<VisualEdge>().Initialize(fromNode, toNode, Color.green);
                    edgeObjects[neighbour].Add(go);
                    go.transform.SetParent(edgeObjectContainer.transform);
                }
            }
        }
    }

    static List<int> GetNodeNeighbourhood(int nodeIndex)
    {
        List<int> neighbourhood = new List<int>();
        Vector2 nodeCoords = GetTileCoords(nodeIndex);

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector2 neighbour = nodeCoords + new Vector2(x, y);
                //check bounds
                if (0 <= neighbour.x && neighbour.x < mapWidth && 0 <= neighbour.y && neighbour.y < mapHeight)
                {
                    neighbourhood.Add(GetTileIndex(neighbour));
                }
                else
                {
                    neighbourhood.Add(-1);
                }
            }
        }
        return neighbourhood;
    }

    static void VisualizeGraph()
    {
        List<GraphNode> nodes = graph.GetNodes();

        //foreach (GraphNode_Demo node in nodes)
        for (int i = 0; i < nodes.Count; i++)
        {
            GraphNode_Demo node = (GraphNode_Demo)nodes[i];
            GameObject go = Instantiate(Config.Instance.node, node.GetPos(), new Quaternion());
            go.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
            nodeObjects.Add(go);
            go.transform.SetParent(nodeObjectContainer.transform);
            if (node.Index() == (int)Nodetype.invalid_node_index)
            {
                go.SetActive(false);
            }
        }

        List<List<GraphEdge>> edges = graph.GetEdges();

        for (int i = 0; i < edges.Count; i++)
        {
            edgeObjects.Add(new List<GameObject>());
            foreach (GraphEdge edge in edges[i])
            {
                GraphNode_Demo fromNode = (GraphNode_Demo)nodes[edge.From()];
                GraphNode_Demo toNode = (GraphNode_Demo)nodes[edge.To()];

                GameObject go = Instantiate(Config.Instance.edge);
                go.GetComponent<VisualEdge>().Initialize(fromNode, toNode, Color.green);
                edgeObjects[i].Add(go);
                go.transform.SetParent(edgeObjectContainer.transform);
            }
        }
    }

    //------------------------ doWallsIntersectCircle -----------------------------
    //
    //  returns true if any walls intersect the circle of radius at point p
    //-----------------------------------------------------------------------------
    public static bool DoWallsIntersectCircle(Vector2 from, Vector2 to, float radius)
    {
        float distance = (to - from).magnitude;
        Vector2 direction = (to - from).normalized;
        int layerMask = LayerMask.GetMask("Wall");
        RaycastHit2D intersectionResult = Physics2D.CircleCast(from, radius, direction, distance, layerMask);
        if (intersectionResult)
        {
            return true;
        }
        return false;
    }
}
