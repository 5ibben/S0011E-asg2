using System.Collections.Generic;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    public static List<GameObject> pathFinderMarkers = new List<GameObject>();
    public static GameObject pathFinderMarkerContainer = new GameObject("Pathfinding Markers");

    Visualizer(){}

    //this is a singleton
    private static Visualizer instance = null;
    public static Visualizer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("Visualizer").AddComponent<Visualizer>();
            }
            return instance;
        }
    }

    public static void AddPathFinderMarker(Vector2 pos)
    {
        pathFinderMarkers.Add(Instantiate(Config.pathFinderNode, pos, Quaternion.identity));
        pathFinderMarkers[pathFinderMarkers.Count - 1].transform.SetParent(pathFinderMarkerContainer.transform);
    }
    public static  void ClearPathFinderMarkers()
    {
        Destroy(pathFinderMarkerContainer);
        pathFinderMarkerContainer = new GameObject("Pathfinding Markers");
        pathFinderMarkers.Clear();
    }
}
