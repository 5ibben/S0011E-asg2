using System.Collections.Generic;
using UnityEngine;
using TMPro;

//public class TestNode : GraphNode
//{
//    public TestNode(int index, Vector2 pos)
//    {
//        m_iIndex = index;
//        this.pos = pos;
//    }

//    public Vector2 GetPos() { return pos; }
//    public void SetPos(Vector2 pos) { this.pos = pos; }
//    Vector2 pos;
//}




public class MainLoop : MonoBehaviour
{
    int gridWidth = 10;
    int gridHeight = 10;
    public TextAsset textMap;
    public GameObject nodeGO;
    public GameObject edgeGO;
    public GameObject pathNodeGO;
    public GameObject pathEdgeGO;

    SparseGraph graph;

    int searchFrom = 0;
    int searchTo = 0;


    void RemoveTest()
    {
        List<GraphEdge> myList = new List<GraphEdge>();

        for (int i = 0; i < 10; i++)
        {
            myList.Add(new GraphEdge(i, i + 1));
        }

        foreach (GraphEdge edge in myList.ToArray())
        {
            if (edge.From() % 2 == 0)
            {
                myList.Remove(edge);
            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        //RemoveTest();

        //GraphTestCreate();
        GraphTestLoad();
        GraphTestDrawNodes();
        PQTest();
    }

    // Update is called once per frame
    void Update()
    {
        //We need to assign this to a variable since the accumulated time is set to zero upon request.
        //Having it directly in the for loop will not work as it will be evaluated every iteration.
        int updates = Clock.Instance.RequestAccumulatedUpdates();
        for (int i = 0; i < updates; i++)
        {
            //character entity update
            //foreach (var character in characters)
            //{
            //    character.EntityUpdate();
            //}
            //dispatch any delayed messages
            MessageDispatcher.Instance.DispatchDelayedMessages();
            //day time update
            Clock.Instance.UpdateElapsedTime();
        }

        UpdateUI();

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;

            Debug.Log("worldPos: " + pos + " nodeIndex: " +  PositionToNodeIndex(pos));
            int nodeIndex = PositionToNodeIndex(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            GameObject go = Instantiate(nodeGO, pos, new Quaternion());
            go.GetComponentInChildren<TextMeshProUGUI>().text = nodeIndex.ToString();
            go.transform.localScale = new Vector3(30, 30, 1);
            //nodeGO.AddComponent<SpriteRenderer>().sprite = nodeSprite;
            go.GetComponent<SpriteRenderer>().color = Color.green;

            searchFrom = PositionToNodeIndex(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
        if (Input.GetMouseButtonDown(1))
        {
            searchTo= PositionToNodeIndex(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            //GraphTestSearchFDS(searchFrom, searchTo);
            GraphTestSearchAdvanced(searchFrom, searchTo);
        }
    }

    void UpdateUI()
    {

    }

    int PositionToNodeIndex(Vector3 pos)
    {
        return ((int)pos.y * gridWidth + (int)pos.x);
    }

    void GraphTestLoad()
    {
        if (textMap)
        {
            graph = (SparseGraph)Graph_Loader.LoadGridGraph(textMap,true,true);
            gridWidth = Graph_Loader.MapWidth();
            gridHeight = Graph_Loader.MapHeight();
        }
    }

    void GraphTestCreate()
    {
        graph = new SparseGraph(false);

        Vector2 offset = new Vector2(0.5f, 0.5f);
        //add nodes
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int nodeIndex = y * gridWidth + x;
                graph.AddNode(new GraphNode_Demo(nodeIndex, new Vector2(x,y) + offset));
            }
        }
        //add horizontal edges
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth - 1; x++)
            {
                int from = y * gridWidth + x;
                int to = from + 1;
                graph.AddEdge(new GraphEdge(from, to));
            }
        }
        //add vertical edges
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight - 1; y++)
            {
                int from = y * gridWidth + x;
                int to = from + gridWidth;
                graph.AddEdge(new GraphEdge(from, to));
            }
        }
    }

    public void GraphTestDrawNodes()
    {
         
        List<GraphNode> nodes = graph.GetNodes();

        //GameObject nodeGO = new GameObject("node");
        nodeGO.GetComponent<SpriteRenderer>().color = Color.cyan;

        foreach (GraphNode_Demo node in nodes)
        {
            if (node.Index() != (int)Nodetype.invalid_node_index)
            {
                GameObject go = Instantiate(nodeGO, node.GetPos(), new Quaternion());
                go.GetComponent<SpriteRenderer>().renderingLayerMask = 2;
                go.GetComponentInChildren<TextMeshProUGUI>().text = node.Index().ToString();
            }
        }

        List<List<GraphEdge>> edges = graph.GetEdges();

        //GameObject edgeGO = new GameObject("edge");
        //edgeGO.AddComponent<LineRenderer>();
        //edgeGO.GetComponent<LineRenderer>().SetWidth(0.2f, 0.0f);
        //edgeGO.GetComponent<LineRenderer>().material = edgeMaterial;
        foreach (List<GraphEdge> edgeList in edges)
        {
            foreach (GraphEdge edge in edgeList)
            {
                GraphNode_Demo fromNode = (GraphNode_Demo)nodes[edge.From()];
                GraphNode_Demo toNode = (GraphNode_Demo)nodes[edge.To()];
                //Vector3[] linePoints = { fromNode.GetPos(), toNode.GetPos() };
                //edgeGO.GetComponent<LineRenderer>().SetPositions(linePoints);
                //edgeGO.GetComponentInChildren<TextMeshProUGUI>().text = node.Index().ToString();
                GameObject go = Instantiate(edgeGO);
                //go.GetComponent<LineRenderer>().sortingOrder = 2;
                go.GetComponent<VisualEdge>().Initialize(fromNode, toNode, Color.green);
            }
        }
    }

    void PQTest()
    {
        List<double> costs = new List<double> { 0, 0, 0, 0, 0, 0 };
        List<int> indexes = new List<int> { 0, 1, 2, 3, 4, 5 };
        costs[2] = 69;
        costs[4] = 7;
        costs[5] = 70;
        IndexedPriorityQueueLow pq = new IndexedPriorityQueueLow(costs, 6);

        //costs[2] = 69;
        pq.insert(2);
        //costs[4] = 7;
        pq.insert(4);
        //costs[5] = 70;
        pq.insert(5);

        costs[2] = 2;
        pq.ChangePriority(2);
        costs[4] = 3;
        pq.ChangePriority(4);
        costs[5] = 1;
        pq.ChangePriority(5);

        for (int i = 0; i < 3; i++)
        {
            Debug.Log("PQ POP: " + pq.Pop());
        }
    }

    void GraphTestSearchAdvanced(int from, int to)
    {
        //Graph_SearchDijkstra search = new Graph_SearchDijkstra(graph, from, to);
        Graph_SearchAStar search = new Graph_SearchAStar(graph, new AStarHeuristic_Euclid(), from, to);
        List<int> path = search.GetPathToTarget();

        Debug.Log("path.Count: " + path.Count);
        if (0 <path.Count)
        {
            List<GraphNode> nodes = graph.GetNodes();

            //visualize edges
            GameObject edgeGO = new GameObject("edge");
            edgeGO.AddComponent<LineRenderer>();
            edgeGO.GetComponent<LineRenderer>().SetWidth(0.02f, 0.02f);
            //edgeGO.GetComponent<LineRenderer>().material = edgeMaterial;
            edgeGO.GetComponent<LineRenderer>().material.color = new Color(0, 255, 0, 255);
            for (int i = 0; i < path.Count - 1; i++)
            {
                GraphNode_Demo fromNode = ((GraphNode_Demo)nodes[path[i]]);
                GraphNode_Demo toNode = ((GraphNode_Demo)nodes[path[i + 1]]);
                Vector3[] linePoints = { fromNode.GetPos(), toNode.GetPos() };
                edgeGO.GetComponent<LineRenderer>().SetPositions(linePoints);
                edgeGO.GetComponent<LineRenderer>().sortingOrder = 1;
                Instantiate(edgeGO);
            }

            foreach (int nodeIndex in path)
            {
                pathNodeGO.GetComponentInChildren<TextMeshProUGUI>().text = nodes[nodeIndex].Index().ToString();
                Instantiate(pathNodeGO, ((GraphNode_Demo)nodes[nodeIndex]).GetPos(), new Quaternion());
            }
        }
        else
        {
            Debug.Log("no path found");
        }
    }


    void GraphTestSearchFDS(int from, int to)
    {
        //Graph_SearchDFS search = new Graph_SearchDFS(graph, from, to);
        Graph_SearchBFS search = new Graph_SearchBFS(graph, from, to);
        
        if (search.Found())
        {
            List<int> path = search.GetPathToTarget();

            List<GraphNode> nodes = graph.GetNodes();

            //visualize edges
            GameObject edgeGO = new GameObject("edge");
            edgeGO.AddComponent<LineRenderer>();
            edgeGO.GetComponent<LineRenderer>().SetWidth(0.02f, 0.02f);
            //edgeGO.GetComponent<LineRenderer>().material = edgeMaterial;
            edgeGO.GetComponent<LineRenderer>().material.color = new Color(0, 255, 0, 255);
            for (int i = 0; i < path.Count-1; i++)
            {
                GraphNode_Demo fromNode = ((GraphNode_Demo)nodes[path[i]]);
                GraphNode_Demo toNode = ((GraphNode_Demo)nodes[path[i+1]]);
                Vector3[] linePoints = { fromNode.GetPos(), toNode.GetPos() };
                edgeGO.GetComponent<LineRenderer>().SetPositions(linePoints);
                edgeGO.GetComponent<LineRenderer>().sortingOrder = 1;
                Instantiate(edgeGO);
            }

            foreach (int nodeIndex in path)
            {
                pathNodeGO.GetComponentInChildren<TextMeshProUGUI>().text = nodes[nodeIndex].Index().ToString();
                Instantiate(pathNodeGO, ((GraphNode_Demo)nodes[nodeIndex]).GetPos(), new Quaternion());
            }
        }
    }
}
