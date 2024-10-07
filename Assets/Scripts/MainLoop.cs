using System.Collections.Generic;
using UnityEngine;

public class TestA : MonoBehaviour
{

}

public class TestB : TestA
{

}

public class TestC : MonoBehaviour
{
    public TestC(TestA test)
    {

    }

    public void Initialize(TestA test)
    {
        myTest = test;
    }

    TestA myTest;
}

public class TestNode : GraphNode
{
    public TestNode(int index, Vector2 pos)
    {
        m_iIndex = index;
        this.pos = pos;
    }

    public Vector3 GetPos() { return pos; }
    public void SetPos(Vector2 pos) { this.pos = pos; }
    Vector2 pos;
}

public class MainLoop : MonoBehaviour
{
    public Sprite nodeSprite;
    public Material edgeMaterial;

    public TestA testA;
    public TestB testB;

    public TestC testC;

    SparseGraph spGraph;

    // Start is called before the first frame update
    void Start()
    {
        testC = new TestC(testA);
        testC.Initialize(testB);

        spGraph = new SparseGraph(true);

        GraphTestCreate();
        GraphTestDrawNodes();
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
    }

    void UpdateUI()
    {

    }

    void GraphTestCreate()
    {
        int width = 2;
        int height = 2;
        //add nodes
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int nodeIndex = y * width + x;
                spGraph.AddNode(new TestNode(nodeIndex, new Vector2(x,y)));
            }
        }
        //add horizontal edges
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width-1; x++)
            {
                int from = y * width + x;
                int to = from + 1;
                spGraph.AddEdge(new GraphEdge(from, to));
            }
        }
        //add vertical edges
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height-1; y++)
            {
                int from = y * width + x;
                int to = from + width;
                spGraph.AddEdge(new GraphEdge(from, to));
            }
        }
    }

    public void GraphTestDrawNodes()
    {
        List<GraphNode> nodes = spGraph.GetNodes();

        GameObject nodeGO = new GameObject("node");
        nodeGO.transform.localScale = new Vector3(50,50,1);
        nodeGO.AddComponent<SpriteRenderer>().sprite = nodeSprite;

        foreach (TestNode node in nodes)
        {
            Instantiate(nodeGO, node.GetPos(), new Quaternion());
        }

        List<List<GraphEdge>> edges = spGraph.GetEdges();

        GameObject edgeGO = new GameObject("edge");
        edgeGO.AddComponent<LineRenderer>();
        edgeGO.GetComponent<LineRenderer>().SetWidth(0.1f, 0.05f);
        edgeGO.GetComponent<LineRenderer>().material = edgeMaterial;
        foreach (List<GraphEdge> edgeList in edges)
        {
            foreach (GraphEdge edge in edgeList)
            {
                TestNode fromNode = (TestNode)nodes[edge.From()];
                TestNode toNode = (TestNode)nodes[edge.To()];
                Vector3[] linePoints = { fromNode.GetPos(), toNode.GetPos() };
                edgeGO.GetComponent<LineRenderer>().SetPositions(linePoints);
                Instantiate(edgeGO);
            }
        }
    }
}
