using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{
    int asg2Start = 0;
    int asg2End = 0;
     Dictionary<char, int> textMaptileCosts = new Dictionary<char, int>();
    Config() 
    {
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

    void Start()
    {
        //AddTextMapDefinitionsAsg2();
    }

    //TODO: load definitions from file
    //text map definitions asg.2
    void AddTextMapDefinitionsAsg2()
    {
        textMaptileCosts.Add('0', 1);
        textMaptileCosts.Add('X', -1);
        textMaptileCosts.Add('S', 1);
        textMaptileCosts.Add('G', 1);

        Debug.Log("textMaptileCosts.count: " + textMaptileCosts.Count);

        asg2Start = 0;
        asg2End = 0;
    }

    public  int GetTextMapTileCost(char tile)
    {
        //Debug.Log("textMaptileCosts.count: " + textMaptileCosts.Count + " GetTextMapTileCost: " + tile + " returning: " + textMaptileCosts.GetValueOrDefault(tile, -69));
        return textMaptileCosts.GetValueOrDefault(tile, -1);
    }
    public const char mapdef_mountain = 'X';
    public const char mapdef_ground = '0';
    //text map definitions asg.4
    //const char mountain = 'm';
    //const char water = 'w';
    //const char ground = 'g';

}
