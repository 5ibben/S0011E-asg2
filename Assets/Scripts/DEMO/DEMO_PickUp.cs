using UnityEngine;

public class DEMO_PickUp : MonoBehaviour
{
    //Hello! I am a pickup of some sorts.
    int itemType = (int)Config.PickUps.PU0;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("pickup collected");
            Destroy(gameObject);
        }
    }
    public int ItemType() { return itemType; }
}
