using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPortalController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.tag);
        //Debug.Log(collision.name);

        if (collision.tag == "Player")
            PlayerController.Interact += Teleport;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            PlayerController.Interact -= Teleport;
    }

    private void Teleport(InventoryObject _)
    {
        GameManager.ToBossFight();
    }
}
