using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

public class InventoryInterface : MonoBehaviour
{
    public GameObject slotPrefab;
    public InventoryObject inventory;
    public Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

    protected void Awake()
    {
        CreateSlots();
        for (int i = 0; i < inventory.GetSlots.Count; i++)
        {
            inventory.GetSlots[i].parent = this;
            inventory.GetSlots[i].OnAfterUpdate += OnSlotUpdate;
            OnSlotUpdate(inventory.GetSlots[i]);
        }
        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
    }

    protected void OnSlotUpdate(InventorySlot _slot)
    {
        if (_slot.item.Id >= 0)
        {
            Debug.Log("Trigger On Slot Update Has item");    
            _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().sprite = _slot.ItemObject.uiDisplay;
            if (_slot.GetLocked())
            {
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.5f);
            }
            else
            {
                _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 1);
            }
        }
        else
        {
            Debug.Log("Trigger On Slot Update No item");

            _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().sprite = null;
            _slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0);
        }
    }

    public void CreateSlots()
    {
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
        for (int i = 0; i < inventory.GetSlots.Count; i++)
        {
            _CreateSlots(i);
        }
    }

    protected virtual void _CreateSlots(int i)
    {
        var obj = Instantiate(slotPrefab, Vector3.zero, Quaternion.identity, transform);

        AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
        AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
        AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
        AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
        AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

        inventory.GetSlots[i].slotDisplay = obj;
        inventory.GetSlots[i].SlotLockStateChange += OnSlotUpdate;

        slotsOnInterface.Add(obj, inventory.GetSlots[i]);
    } 

    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }

    public void OnEnter(GameObject obj)
    {
        MouseData.slotHoveredOver = obj;
        Debug.Log("Enter");
        StopAllCoroutines();
        Debug.Log(slotsOnInterface[obj].item.Name);
        StartCoroutine(StartHoverTimer(slotsOnInterface[obj].item.Name));
    }
    public void OnExit(GameObject obj)
    {
        MouseData.slotHoveredOver = null;
        Debug.Log("Exit");
        StopAllCoroutines();
        HoverUIManager.OnMouseLoseFocus();
    }
    public void OnEnterInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = obj.GetComponent<InventoryInterface>();
    }
    public void OnExitInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = null;
    }
    public void OnDragStart(GameObject obj)
    {
        if (slotsOnInterface[obj].GetLocked())
        {
            return;
        }
        MouseData.tempItemBeingDragged = CreateTempItem(obj);
    }
    public GameObject CreateTempItem(GameObject obj)
    {
        GameObject tempItem = null;
        if (slotsOnInterface[obj].item.Id >= 0)
        {
            tempItem = new GameObject();
            var rt = tempItem.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50, 50);
            tempItem.transform.SetParent(transform.parent);
            var img = tempItem.AddComponent<Image>();
            img.sprite = slotsOnInterface[obj].ItemObject.uiDisplay;
            img.raycastTarget = false;
        }
        return tempItem;
    }
    public void OnDragEnd(GameObject obj)
    {
        Destroy(MouseData.tempItemBeingDragged);
        if (MouseData.interfaceMouseIsOver == null)
        {
            slotsOnInterface[obj].DropItem();
            return;
        }
        if (MouseData.slotHoveredOver && slotsOnInterface[obj].GetLocked())
        {
            return;
        }
        if (MouseData.slotHoveredOver && slotsOnInterface[obj].item.Id >= 0)
        {
            InventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver];
            inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
        }
    }
    public void OnDrag(GameObject obj)
    {
        if (MouseData.tempItemBeingDragged != null)
            MouseData.tempItemBeingDragged.GetComponent<RectTransform>().position = Input.mousePosition;
    }

    private IEnumerator StartHoverTimer(string content)
    {
        Debug.Log("StartHoverTimer");

        yield return new WaitForSecondsRealtime(0.5f);

        Debug.Log("End Hover Timer");

        HoverUIManager.OnMouseHover(content, Input.mousePosition);
    }

    private void OnDestroy()
    {
        Destroy(MouseData.tempItemBeingDragged);
        for (int i = 0; i < inventory.GetSlots.Count; i++)
        {
            inventory.GetSlots[i].OnAfterUpdate -= OnSlotUpdate;
            inventory.GetSlots[i].SlotLockStateChange -= OnSlotUpdate;
        }
    }

    private void OnApplicationQuit()
    {
        for (int i = 0; i < inventory.GetSlots.Count; i++)
        {
            inventory.GetSlots[i].OnAfterUpdate -= OnSlotUpdate;
        }
    }
}

public static class MouseData
{
    public static InventoryInterface interfaceMouseIsOver;
    public static GameObject tempItemBeingDragged;
    public static GameObject slotHoveredOver;
}


public static class ExtensionMethods
{
    public static void UpdateSlotDisplay(this Dictionary<GameObject, InventorySlot> _slotsOnInterface)
    {
        foreach (KeyValuePair<GameObject, InventorySlot> _slot in _slotsOnInterface)
        {
            if (_slot.Value.item.Id >= 0)
            {
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = _slot.Value.ItemObject.uiDisplay;
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 1);
            }
            else
            {
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = null;
                _slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0);
                _slot.Key.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
        }
    }
}
