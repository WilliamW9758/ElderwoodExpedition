using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemWorld : MonoBehaviour, ISerializationCallbackReceiver
{
    public ItemObject item;

    private void Awake()
    {
        
    }

    private void Start()
    {
        GetComponentInChildren<SpriteRenderer>().sprite = item.uiDisplay;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.tag);
        //Debug.Log(collision.name);

        if (collision.tag == "Player")
            PlayerController.Interact += PickUp;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            PlayerController.Interact -= PickUp;
    }

    public void PickUp(InventoryObject inv)
    {
        inv.AddItem(new Item(item));
        Destroy(gameObject);
    }

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        try
        {
            GetComponentInChildren<SpriteRenderer>().sprite = item.uiDisplay;
            EditorUtility.SetDirty(GetComponentInChildren<SpriteRenderer>());
        } catch (System.NullReferenceException)
        {

        }
#endif
    }

    public void OnAfterDeserialize()
    {

    }
}
