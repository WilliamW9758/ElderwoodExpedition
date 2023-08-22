using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public Sprite chestOpen;
    public GameObject itemWorldPrefab;
    public ItemObject[] PossibleItems;
    public ItemDatabaseObject database;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            PlayerController.Interact += Open;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            PlayerController.Interact -= Open;
    }

    private void Open(InventoryObject _)
    {
        int item = Random.Range(0, PossibleItems.Length);
        GameObject itemWorld = Instantiate(itemWorldPrefab, transform.position, Quaternion.identity);
        itemWorld.GetComponent<ItemWorld>().item = database.ItemObjects[PossibleItems[item].data.Id];
        GetComponent<SpriteRenderer>().sprite = chestOpen;
        PlayerController.Interact -= Open;
        GetComponent<Collider2D>().enabled = false;
    }
}
