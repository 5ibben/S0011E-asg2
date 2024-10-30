using System.Collections.Generic;
using UnityEngine;

public class DEMO_Game : MonoBehaviour
{
    //this class manages all the path planning requests
    PathManager m_pPathManager;

    List<DEMO_Character> characters = new List<DEMO_Character>();
    public DEMO_Character selectedCharacter;

    // Start is called before the first frame update
    private void Awake()
    {
        Debug.Log("DEMO_Game awake");
        InitializeMap(0);
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
            foreach (var character in characters)
            {
                character.EntityUpdate();
            }
            //update pathfinding
            m_pPathManager.UpdateSearches();
            //dispatch any delayed messages
            MessageDispatcher.Instance.DispatchDelayedMessages();
            //day time update
            Clock.Instance.UpdateElapsedTime();
        }
    }

    public void InitializeMap(uint map)
    {
        MessageDispatcher.Flush();
        m_pPathManager = new PathManager();
        //load map
        Config.Instance.GetTextMap(0);
        if (Config.Instance.GetTextMap(0))
        {
            DEMO_Map.LoadGridMap(Config.Instance.GetTextMap(map));
        }
        //spawn characters
        characters.Add(Instantiate(Config.Instance.player, DEMO_Map.GetSpawnPoints()[0], Quaternion.identity).GetComponent<DEMO_Character>());
        characters[0].GetComponent<DEMO_Character>().Initialize(this);
        //register characters with the entity manager
        foreach (var character in characters)
        {
            character.EntityStart();
            EntityManager.Instance.RegisterEntity(character);
        }
        //select a character
        selectedCharacter = characters[0];
    }

    public void CleanUpCurrentMap()
    {
        selectedCharacter = null;
        //unregister characters from the entity manager
        for (int i = characters.Count-1; i >= 0; i--)
        {
            EntityManager.Instance.RemoveEntity(characters[i]);
            characters[i].EntityDestroy();
            Destroy(characters[i].gameObject);
        }
        characters.Clear();
        //load map
        Config.Instance.GetTextMap(0);
        if (Config.Instance.GetTextMap(0))
        {
            Debug.Log("DEMO_Game UnLoadGridMap");
            DEMO_Map.UnLoadGridMap();
        }
    }

    public void SelectedCharacterMove(Vector2 destination)
    {
        MessageDispatcher.DispatchMessage(MessageDispatcher.SEND_MSG_IMMEDIATELY, MessageDispatcher.SENDER_ID_IRRELEVANT, selectedCharacter.ID(), (int)messages.Msg_Character_MoveToPosition, new ExtraInfo_Character_MoveToPosition(destination));
    }

    public void SelectedCharacterFindItem()
    {
        MessageDispatcher.DispatchMessage(MessageDispatcher.SEND_MSG_IMMEDIATELY, MessageDispatcher.SENDER_ID_IRRELEVANT, selectedCharacter.ID(), (int)messages.Msg_Character_FindItem, new ExtraInfo_Character_FindItem((int)Config.PickUps.PU0));
    }

    public PathManager GetPathManager()
    {
        return m_pPathManager;
    }

    public void PaintWalls(Vector3 worldPos)
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
            //inform characters of graph change
            foreach (var character in characters)
            {
                MessageDispatcher.DispatchMessage(MessageDispatcher.SEND_MSG_IMMEDIATELY, MessageDispatcher.SENDER_ID_IRRELEVANT, character.ID(), (int)messages.Msg_GraphChanged, new ExtraInfo_GraphChanged(tile));
            }
        }
    }

    public void PaintPickups(Vector3 worldPos)
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
                foreach (var character in characters)
                {
                    MessageDispatcher.DispatchMessage(MessageDispatcher.SEND_MSG_IMMEDIATELY, MessageDispatcher.SENDER_ID_IRRELEVANT, character.ID(), (int)messages.Msg_GraphItemChanged, new ExtraInfo_GraphItemChanged(tile));
                }
            }
            else if (DEMO_Map.GetTileObject(tile).gameObject.name == "wall(Clone)")
            {
                DEMO_Map.AddTileObject('0', tile);
                DEMO_Map.AddWorldObject(Config.GetTextMapWorldObject('G'), tile);
                //inform characters of graph change
                foreach (var character in characters)
                {
                    MessageDispatcher.DispatchMessage(MessageDispatcher.SEND_MSG_IMMEDIATELY, MessageDispatcher.SENDER_ID_IRRELEVANT, character.ID(), (int)messages.Msg_GraphChanged, new ExtraInfo_GraphChanged(tile));
                }
            }
        }
    }
}
