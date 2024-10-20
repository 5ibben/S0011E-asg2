using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DEMO_PickUp2 : DEMO_PickUp
{

}

public class MainLoop : MonoBehaviour
{
    public Button paintButton;
    //int gridWidth = 10;
    //int gridHeight = 10;
    public TextAsset textMap;
    public GameObject nodeGO;
    public GameObject edgeGO;
    public GameObject pathNodeGO;
    public GameObject pathEdgeGO;

    List<GameObject> characters = new List<GameObject>();
    GameObject selectedCharacter;

    int searchFrom = 0;
    int searchTo = 0;

    bool paintPickups = false;
    int previousPaintTile = -1;


    // Start is called before the first frame update
    void Start()
    {
        GraphNode n0 = new GraphNode();
        DEMO_PickUp dp0 = new DEMO_PickUp();
        DEMO_PickUp2 dp1 = new DEMO_PickUp2();

        Debug.Log("n0.GetType() == dp0.GetType()" + (n0.GetType() == dp0.GetType()));
        Debug.Log("dp1.GetType() == dp0.GetType()" + (dp1.GetType() == dp0.GetType()));

        for (int i = 0; i < 9; i++)
        {
            Debug.Log("i : i % 2 " + i + ":" + i % 2);
        }

        //load map
        if (textMap)
        {
            DEMO_Map.LoadGridMap(textMap, true, true, true);
        }

        //spawn characters
        characters.Add(Instantiate(Config.Instance.player, DEMO_Map.GetSpawnPoints()[0], Quaternion.identity));
        //select character
        selectedCharacter = characters[0];
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

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector3 gridWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0.5f, 0.5f, 0);
        //    if (paintPickups)
        //    {
        //        PaintPickups(gridWorldPos);
        //    }
        //    else
        //    {
        //        PaintWalls(gridWorldPos);
        //    }

        //}
        if (Input.GetMouseButton(0))
        {
            Vector3 gridWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0.5f, 0.5f, 0);
            int paintTile = DEMO_Map.GetTileIndexSafe(gridWorldPos);
            if (paintTile != previousPaintTile)
            {
                previousPaintTile = paintTile;
                if (paintPickups)
                {
                    PaintPickups(gridWorldPos);
                }
                else
                {
                    PaintWalls(gridWorldPos);
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            previousPaintTile = -1;
        }
        if (Input.GetMouseButtonDown(1))
        {
            //Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //DEMO_Map.DoWallsIntersectCircle(selectedCharacter.transform.position, worldPos, 0.1f);

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectedCharacter.GetComponent<DEMO_Character>().MoveCharacter(worldPos);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            selectedCharacter.GetComponent<DEMO_Character>().FindItem(new DEMO_PickUp());
        }
    }

    public void TogglePaintMode()
    {
        paintPickups = !paintPickups;
    }

    void PaintWalls(Vector3 worldPos)
    {
        int tile = DEMO_Map.GetTileIndexSafe(worldPos);
        if (tile != -1)
        {
            if (DEMO_Map.GetTileObject(tile).gameObject.name == "floor(Clone)")
            {
                //remove potential worldobject
                DEMO_Map.RemoveWorldObject(tile);
                //Add wall
                DEMO_Map.AddTileObject('X', tile);
            }
            else if (DEMO_Map.GetTileObject(tile).gameObject.name == "wall(Clone)")
            {
                DEMO_Map.AddTileObject('0', tile);
            }
        }
    }

    void PaintPickups(Vector3 worldPos)
    {
        int tile = DEMO_Map.GetTileIndexSafe(worldPos);
        if (tile != -1)
        {
            if (DEMO_Map.GetTileObject(tile).gameObject.name == "floor(Clone)")
            {
                if (DEMO_Map.GetWorldObject(tile) is null)
                {
                    DEMO_Map.AddWorldObject(Config.GetTextMapWorldObject('G'), tile);
                }
                else
                {
                    DEMO_Map.RemoveWorldObject(tile);
                }
            }
            else if (DEMO_Map.GetTileObject(tile).gameObject.name == "wall(Clone)")
            {
                DEMO_Map.AddTileObject('0', tile);
                DEMO_Map.AddWorldObject(Config.GetTextMapWorldObject('G'), tile);
            }
        }
    }
}
