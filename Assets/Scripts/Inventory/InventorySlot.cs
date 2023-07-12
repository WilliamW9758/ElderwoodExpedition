using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [HideInInspector]
    public InventoryManager inventoryManager;
    public bool hasItem;

    private void Awake()
    {
        inventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
    }

    private void Update()
    {
        if (transform.Find("ItemSlot").childCount > 0)
        {
            hasItem = true;
        } else
        {
            hasItem = false;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        ItemController item = dropped.GetComponent<ItemController>();
        Debug.Log(transform.parent.name);
        Debug.Log(item.thisItem.IsWeaponPart);

        if ((transform.parent.name == "WeaponLeft" ||
            transform.parent.name == "WeaponRight") &&
            item.thisItem.IsWeaponPart == false)
        {
            return;
        }
        if (!hasItem)
        {
            item.parentAfterDrag = transform.Find("ItemSlot");
        }
        else
        {
            Transform prevItem = transform.Find("ItemSlot").GetChild(0);
            prevItem.SetParent(item.parentAfterDrag);
            prevItem.SetAsFirstSibling();
            item.parentAfterDrag = transform.Find("ItemSlot");
        }
        inventoryManager.ContentUpdate();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked");
        Debug.Log(transform.parent.name);
        if (eventData.button == PointerEventData.InputButton.Left && Input.GetKey(KeyCode.LeftShift) &&
            hasItem)
        {
            if (transform.parent.name == "InventoryGrid")
            {
                GameObject weaponSlot = inventoryManager.NextAvailableWeaponLeftSlot();
                if (!weaponSlot) { weaponSlot = inventoryManager.NextAvailableWeaponRightSlot(); }
                if (weaponSlot)
                {
                    Transform itemToMove = transform.Find("ItemSlot").GetChild(0);
                    if (itemToMove.GetComponent<ItemController>().thisItem.IsWeaponPart)
                    {
                        hasItem = false;
                        weaponSlot.GetComponent<InventorySlot>().hasItem = true;
                        itemToMove.GetComponent<ItemController>().parentAfterDrag = weaponSlot.transform.Find("ItemSlot");
                        itemToMove.SetParent(weaponSlot.transform.Find("ItemSlot"));
                        itemToMove.SetAsFirstSibling();
                    }
                }
            }
            else if (transform.parent.name == "WeaponLeft" || transform.parent.name == "WeaponRight")
            {
                if (!inventoryManager.inventoryObject.activeSelf)
                {
                    return;
                }
                GameObject inventorySlot = inventoryManager.NextAvailableInventorySlot();
                if (inventorySlot)
                {
                    hasItem = false;
                    inventorySlot.GetComponent<InventorySlot>().hasItem = true;
                    Transform itemToMove = transform.Find("ItemSlot").GetChild(0);
                    itemToMove.GetComponent<ItemController>().parentAfterDrag = inventorySlot.transform.Find("ItemSlot");
                    itemToMove.SetParent(inventorySlot.transform.Find("ItemSlot"));
                    itemToMove.SetAsFirstSibling();
                }
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right && Input.GetKey(KeyCode.LeftShift) &&
            hasItem)
        {
            if (transform.parent.name == "InventoryGrid")
            {
                GameObject weaponSlot = inventoryManager.NextAvailableWeaponRightSlot();
                if (!weaponSlot) { weaponSlot = inventoryManager.NextAvailableWeaponLeftSlot(); }
                if (weaponSlot)
                {
                    Transform itemToMove = transform.Find("ItemSlot").GetChild(0);
                    if (itemToMove.GetComponent<ItemController>().thisItem.IsWeaponPart)
                    {
                        hasItem = false;
                        weaponSlot.GetComponent<InventorySlot>().hasItem = true;
                        itemToMove.GetComponent<ItemController>().parentAfterDrag = weaponSlot.transform.Find("ItemSlot");
                        itemToMove.SetParent(weaponSlot.transform.Find("ItemSlot"));
                        itemToMove.SetAsFirstSibling();
                    }
                }
            }
            else if (transform.parent.name == "WeaponLeft" || transform.parent.name == "WeaponRight")
            {
                if (!inventoryManager.inventoryObject.activeSelf)
                {
                    return;
                }
                GameObject inventorySlot = inventoryManager.NextAvailableInventorySlot();
                if (inventorySlot)
                {
                    hasItem = false;
                    inventorySlot.GetComponent<InventorySlot>().hasItem = true;
                    Transform itemToMove = transform.Find("ItemSlot").GetChild(0);
                    itemToMove.GetComponent<ItemController>().parentAfterDrag = inventorySlot.transform.Find("ItemSlot");
                    itemToMove.SetParent(inventorySlot.transform.Find("ItemSlot"));
                    itemToMove.SetAsFirstSibling();
                }
            }
        }
        inventoryManager.ContentUpdate();
    }
}
