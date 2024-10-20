using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEMO_Character : BaseGameEntity
{
    public GameObject pathMarker;

    DEMO_PathPlanner pathPlanner;

    List<Vector3> currentPath = new List<Vector3>();
    List<GameObject> pathMarkers = new List<GameObject>();

    public float movementSpeed = 1.0f;
    float boundingRadius = 0.4f;
    float movementDistance = 0;
    float movementLerp = 0;
    int pathIndex = 0;

    public float BoundingRadius()
    { 
        return boundingRadius;
    }

    // Start is called before the first frame update
    void Start()
    {
        pathPlanner = new DEMO_PathPlanner(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (pathIndex < currentPath.Count)
        {
            movementLerp += movementSpeed / (movementDistance+0.001f) * Time.deltaTime;
            transform.position = Vector3.Lerp(currentPath[pathIndex - 1], currentPath[pathIndex], movementLerp);

            while (1.0f < movementLerp)
            {
                movementLerp -= 1.0f;
                pathIndex++;
                UpdateHeading(pathIndex);
            }
        }
    }

    void UpdateHeading(int pathIndex)
    {
        if (pathIndex < currentPath.Count)
        {
            Vector3 direction = currentPath[pathIndex] - currentPath[pathIndex - 1];
            movementDistance = direction.magnitude;
            transform.up = direction.normalized;
        }
    }

    public void MoveCharacter(Vector2 pos)
    {
        //get path
        currentPath.Clear();
        if (pathPlanner.CreatePathToPosition(pos, currentPath))
        {
            currentPath = pathPlanner.SmoothPathQuick(currentPath);
            //reset path values
            pathIndex = 1;
            movementLerp = 0;
            UpdateHeading(pathIndex);
            //visualize
            VisualizePath();
        }
    }

    public void FindItem(DEMO_PickUp item)
    {
        //get path
        currentPath.Clear();
        if (pathPlanner.RequestPathToItem(item, currentPath))
        {
            currentPath = pathPlanner.SmoothPathQuick(currentPath);
            //reset path values
            pathIndex = 1;
            movementLerp = 0;
            UpdateHeading(pathIndex);
            //visualize
            VisualizePath();
        }
    }

    public void VisualizePath()
    {
        foreach (GameObject marker in pathMarkers)
        {
            Destroy(marker);
        }
        pathMarkers.Clear();
        foreach (Vector2 point in currentPath)
        {
            pathMarkers.Add(Instantiate(pathMarker, point, Quaternion.identity));
        }
    }

    public override void EntityUpdate() 
    {

    }
}
