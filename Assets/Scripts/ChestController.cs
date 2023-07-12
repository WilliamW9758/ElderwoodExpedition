using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public bool locked = false;

    public List<GameObject> possibleItems;

    private void Start()
    {
        if (!locked)
        {
            OpenChest();
        }
    }

    public virtual void OpenChest()
    {
        Instantiate(possibleItems[Random.Range(0, possibleItems.Count)],
            transform.position, Quaternion.identity);
    }

}
