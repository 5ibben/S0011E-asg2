using System.Collections.Generic;
using UnityEngine;

class EntityManager : MonoBehaviour
{
    Dictionary<int, BaseGameEntity> m_EntityMap = new Dictionary<int, BaseGameEntity>();

    EntityManager() { }

    //this class is a singleton
    private static EntityManager instance = null;
    public static EntityManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("EntityManager").AddComponent<EntityManager>();
                //instance = new EntityManager();
            }
            return instance;
        }
    }

    public Dictionary<int, BaseGameEntity> GetEntities()
    {
        return m_EntityMap;
    }

    public void RegisterEntity(BaseGameEntity NewEntity)
    {
        Clock.Instance.ElapsedTime();
        Debug.Log("adding new entity: ");
        Debug.Log("ID : " + NewEntity.ID());
        Debug.Log("Entity : " + NewEntity.name);
        m_EntityMap.Add(NewEntity.ID(), NewEntity);
    }

    public BaseGameEntity GetEntityFromID(int id)
    {
        //find the entity
        m_EntityMap.TryGetValue(id, out BaseGameEntity bge);

        return bge;
    }

    //this method removes the entity from the list
    public void RemoveEntity(BaseGameEntity pEntity)
    {
        m_EntityMap.Remove(pEntity.ID());
    }
};
