using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEMO_PickUp : BaseGameEntity
{
    //Hello! I am a pickup of some sorts.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("pickup collected");
            Destroy(gameObject);
        }
    }
}
