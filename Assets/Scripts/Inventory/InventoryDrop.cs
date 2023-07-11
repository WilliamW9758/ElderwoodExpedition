using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDrop : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        ItemController item = dropped.GetComponent<ItemController>();
        item.parentAfterDrag = null;
        dropped.transform.position = GameObject.FindGameObjectWithTag("Player").transform.position;
    }
}
