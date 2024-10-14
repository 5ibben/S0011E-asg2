using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph_Loader : MonoBehaviour
{
    Graph_Loader() { }
    //this is a singleton
    private static Graph_Loader instance = null;
    public static Graph_Loader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject().AddComponent<Graph_Loader>();
            }
            return instance;
        }
    }

    static int mapWidth = 0;
    static int mapHeight = 0;

    public static int MapWidth() { return mapWidth; }
    public static int MapHeight() { return mapHeight; }

    static void CalculateGridDimensions(TextAsset map)
    {
        string mapstring = map.ToString();

        int width = 0;
        while (mapstring[width] != '\n')
        {
            width++;
        }
        //-1 for newline character
        mapWidth = width-1;
        mapHeight = mapstring.Length / width;

        Debug.Log("mapWidth" + mapWidth);
        Debug.Log("mapHeight" + mapHeight);
    }

    static List<int> GetNeighbours(string map, int nodeX, int nodeY, bool include_diagonals)
    {
        bool cullCornerCutters = true;
        List<int> neighbours = new List<int>();
        string mapstring = map.ToString();

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                int neigbourX = nodeX + x;
                int neigbourY = nodeY + y;

                //check bounds
                if (0 <= neigbourX && neigbourX < mapWidth && 0 <= neigbourY && neigbourY < mapHeight)
                {
                    //don't add self
                    if (x != 0 || y != 0)
                    {
                        int neighbour = neigbourY * mapWidth + neigbourX;
                        if (neigbourX == nodeX || neigbourY == nodeY)
                        {
                            neighbours.Add(neighbour);
                        }
                        else if (include_diagonals)//diagonals
                        {
                            if (cullCornerCutters)
                            {
                                int horizontalNeighbour = nodeY * mapWidth + neigbourX;
                                int verticalNeighbour = neigbourY * mapWidth + nodeX;
                                int horizontalNeighbourCost = Config.Instance.GetTextMapTileCost(mapstring[horizontalNeighbour]);
                                int verticalNeighbourCost = Config.Instance.GetTextMapTileCost(mapstring[verticalNeighbour]);
                                if (0 <= horizontalNeighbourCost && 0 <= verticalNeighbourCost)
                                {
                                    neighbours.Add(neighbour);
                                }
                                else
                                {
                                    //Debug.Log("for node: (" + nodeY * mapWidth + nodeX + "):");
                                    //Debug.Log("\tculling neigbour: (" + neighbour + ")");
                                    //Debug.Log("\ton accopunt of horizontal neigbour: (" + horizontalNeighbour + ") being: " + horizontalNeighbourCost +
                                    //    " and vertical neighbour: (" + verticalNeighbour + ") being: " + verticalNeighbourCost);
                                    //Debug.Log("for node: (" + nodeX + "," + nodeY + "):");
                                    //Debug.Log("\tculling neigbour: (" + neigbourX + "," + neigbourY + ")");
                                    //Debug.Log("\ton accopunt of horizontal neigbour: (" + neigbourX + "," + nodeY + ") being: " + horizontalNeighbourCost +
                                    //    " and vertical neighbour: (" + nodeX + "," + neigbourY + ") being: " + verticalNeighbour);
                                }
                            }
                            else
                            {
                                neighbours.Add(neighbour);
                            }
                        }
                    }
                }
            }
        }
        return neighbours;
    }

    static void GenerateEdges(SparseGraph graph, string mapstring, bool include_diagonals)
    {
        for (int nodeIndex = 0; nodeIndex < mapstring.Length; nodeIndex++)
        {
            int nodeCost = Config.Instance.GetTextMapTileCost(mapstring[nodeIndex]);
            //flip y to be in the same direction as a text file
            //int y = mapHeight - nodeIndex / (mapWidth);
            int nodeY = nodeIndex / (mapWidth);
            int nodeX = nodeIndex % (mapWidth);

            bool cullCornerCutters = true;

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    int neigbourX = nodeX + x;
                    int neigbourY = nodeY + y;

                    //check bounds
                    if (0 <= neigbourX && neigbourX < mapWidth && 0 <= neigbourY && neigbourY < mapHeight)
                    {
                        //don't add self
                        if (x != 0 || y != 0)
                        {
                            int neighbour = neigbourY * mapWidth + neigbourX;
                            int neighbourCost = Config.Instance.GetTextMapTileCost(mapstring[neighbour]);
                            if (neigbourX == nodeX || neigbourY == nodeY)
                            {
                                double edgeCost = ((nodeCost + neighbourCost) / 2);
                                graph.AddEdge(new GraphEdge(nodeIndex, neighbour, edgeCost));
                            }
                            else if (include_diagonals)//diagonals
                            {
                                double edgeCost = ((nodeCost + neighbourCost) / 2) * 1.4;

                                if (cullCornerCutters)
                                {
                                    int horizontalNeighbour = nodeY * mapWidth + neigbourX;
                                    int verticalNeighbour = neigbourY * mapWidth + nodeX;
                                    int horizontalNeighbourCost = Config.Instance.GetTextMapTileCost(mapstring[horizontalNeighbour]);
                                    int verticalNeighbourCost = Config.Instance.GetTextMapTileCost(mapstring[verticalNeighbour]);

                                    if (0 <= horizontalNeighbourCost && 0 <= verticalNeighbourCost)
                                    {
                                        graph.AddEdge(new GraphEdge(nodeIndex, neighbour, edgeCost));
                                    }
                                    else
                                    {
                                        //Debug.Log("for node: (" + nodeY * mapWidth + nodeX + "):");
                                        //Debug.Log("\tculling neigbour: (" + neighbour + ")");
                                        //Debug.Log("\ton accopunt of horizontal neigbour: (" + horizontalNeighbour + ") being: " + horizontalNeighbourCost +
                                        //    " and vertical neighbour: (" + verticalNeighbour + ") being: " + verticalNeighbourCost);
                                        //Debug.Log("for node: (" + nodeX + "," + nodeY + "):");
                                        //Debug.Log("\tculling neigbour: (" + neigbourX + "," + neigbourY + ")");
                                        //Debug.Log("\ton accopunt of horizontal neigbour: (" + neigbourX + "," + nodeY + ") being: " + horizontalNeighbourCost +
                                        //    " and vertical neighbour: (" + nodeX + "," + neigbourY + ") being: " + verticalNeighbour);
                                    }
                                }
                                else
                                {
                                    graph.AddEdge(new GraphEdge(nodeIndex, neighbour, edgeCost));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public static string mapFlipXY(string mapString)
    {
        char[] flippedArray = new char[mapString.Length];
        int flippedIndex = 0;
        for (int i = mapString.Length-1; i >= 0; i--)
        {
            flippedArray[flippedIndex++] = mapString[i];
        }
        return new string(flippedArray);
    }

    public static string mapFlipY(string mapString)
    {
        string flippedString = new string("");
        for (int i = mapHeight - 1; i >= 0; i--)
        {
            flippedString += mapString.Substring(i * mapWidth, mapWidth);
        }
        return flippedString;
    }

    public static Graph LoadGridGraph(TextAsset map, bool flipX, bool flipY)
    {
        CalculateGridDimensions(map);
        string mapstring = map.ToString();
        //remove junk
        mapstring = mapstring.Replace("\n", "");
        mapstring = mapstring.Replace("\r", "");

        if (flipX)
        {
            mapstring = mapFlipXY(mapstring);
            if (!flipY)
            {
                mapstring = mapFlipY(mapstring);
            }
        }
        else if(flipY)
        {
            mapstring = mapFlipY(mapstring);
        }


        SparseGraph graph = new SparseGraph(false);

        Debug.Log("Loading gridMap. Width: " + mapWidth + " Height:" + mapHeight + " Count: " + mapstring.Length);
        //add nodes
        int nodeIndexGraph = 0;
        for (int nodeIndex = 0; nodeIndex < mapstring.Length; nodeIndex++)
        {
            int nodeCost = Config.Instance.GetTextMapTileCost(mapstring[nodeIndex]);
            //flip y to be in the same direction as a text file
            //int y = mapHeight - nodeIndex / (mapWidth);
            int y = nodeIndex / (mapWidth);
            int x = nodeIndex % (mapWidth);
            Vector2 offset = new Vector2(0.5f, 0.5f);
            graph.AddNode(new GraphNode_Demo(nodeIndexGraph++, new Vector2(x, y) + offset));

        }

        //add edges
        GenerateEdges(graph, mapstring, true);

        //remove unwalkable nodes
        for (int nodeIndex = 0; nodeIndex < mapstring.Length; nodeIndex++)
        {
            int nodeCost = Config.Instance.GetTextMapTileCost(mapstring[nodeIndex]);
            //flip y to be in the same direction as a text file
            //int y = mapHeight - nodeIndex / (mapWidth);
            int y = nodeIndex / (mapWidth);
            int x = nodeIndex % (mapWidth);
            if (nodeCost < 0)
            {
                graph.RemoveNode(nodeIndex);
            }
        }


        return graph;

        ////add nodes
        //for (int y = 0; y < gridHeight; y++)
        //{
        //    for (int x = 0; x < gridWidth; x++)
        //    {
        //        int nodeIndex = y * gridWidth + x;
        //        spGraph.AddNode(new TestNode(nodeIndex, new Vector2(x, y) + offset));
        //    }
        //}
        ////add horizontal edges
        //for (int y = 0; y < gridHeight; y++)
        //{
        //    for (int x = 0; x < gridWidth - 1; x++)
        //    {
        //        int from = y * gridWidth + x;
        //        int to = from + 1;
        //        spGraph.AddEdge(new GraphEdge(from, to));
        //    }
        //}
        ////add vertical edges
        //for (int x = 0; x < gridWidth; x++)
        //{
        //    for (int y = 0; y < gridHeight - 1; y++)
        //    {
        //        int from = y * gridWidth + x;
        //        int to = from + gridWidth;
        //        spGraph.AddEdge(new GraphEdge(from, to));
        //    }
        //}
    }
}
