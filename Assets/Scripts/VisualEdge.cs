using UnityEngine;
using TMPro;

public class VisualEdge : MonoBehaviour
{
    public TextMeshProUGUI from;
    public TextMeshProUGUI to;
    public void Initialize(GraphNode_Demo start, GraphNode_Demo end, Color color)
    {
        Vector2 edgeDir = (end.GetPos() - start.GetPos());
        Vector3[] linePoints = { start.GetPos(), end.GetPos() - edgeDir * 0.24f };
        GetComponent<LineRenderer>().SetPositions(linePoints);
        from.gameObject.transform.position = start.GetPos() + edgeDir *0.3f;
        to.gameObject.transform.position = end.GetPos() - edgeDir * 0.3f;
        from.text = start.Index().ToString();
        to.text = end.Index().ToString();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
