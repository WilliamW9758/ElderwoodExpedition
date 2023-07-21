using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;
using System.Runtime.Serialization;
using NaughtyAttributes;
using UnityEngine.Events;

public enum InterfaceType
{
    Inventory,
    Weapon,
    Chest
}

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    public string savePath;
    public ItemDatabaseObject database;
    public InterfaceType type;
    public UnityAction<float> startReload;
    private float reloadMax;
    public float ReloadMax {
        get { return reloadMax;}
        set {
            reloadMax = value;
        }
    }
    public Inventory Container;
    public List<InventorySlot> GetSlots { get { return Container.Slots; } }
    public int GetSize { get { return Container.inventorySize; } }

    public ItemType[] InvenotryAllowedItems = new ItemType[0];

    public bool AddItem(Item _item)
    {
        if (EmptySlotCount <= 0)
            return false;
        SetEmptySlot(_item);
        return true;
    }

    public int GetFirstNonEmptySlotIndex
    {
        get
        {
            for (int i = 0; i < GetSize; i++)
            {
                if (GetSlots[i].item.Id >= 0)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    public int EmptySlotCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < GetSize; i++)
            {
                if (GetSlots[i].item.Id <= -1)
                {
                    counter++;
                }
            }
            return counter;
        }
    }

    public InventorySlot FindItemOnInventory(Item _item)
    {
        for (int i = 0; i < GetSize; i++)
        {
            if (GetSlots[i].item.Id == _item.Id)
            {
                return GetSlots[i];
            }
        }
        return null;
    }
    public InventorySlot SetEmptySlot(Item _item)
    {
        for (int i = 0; i < GetSize; i++)
        {
            if (GetSlots[i].item.Id <= -1)
            {
                GetSlots[i].UpdateSlot(_item);
                return GetSlots[i];
            }
        }
        //set up functionality for full inventory
        return null;
    }

    public void SwapItems(InventorySlot item1, InventorySlot item2)
    {
        if (item2.CanPlaceInSlot(item1.ItemObject) && item1.CanPlaceInSlot(item2.ItemObject))
        {
            InventorySlot temp = new InventorySlot(item2.item);
            item2.UpdateSlot(item1.item);
            item1.UpdateSlot(temp.item);
        }
    }


    [Button]
    public void Save()
    {
        //string saveData = JsonUtility.ToJson(this, true);
        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
        //bf.Serialize(file, saveData);
        //file.Close();
        Debug.Log("Inventory Saved");
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, Container);
        stream.Close();
    }
    [Button]
    public void Load()
    {
        if (File.Exists(string.Concat(Application.persistentDataPath, savePath)))
        {
            //BinaryFormatter bf = new BinaryFormatter();
            //FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open);
            //JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), this);
            //file.Close();
            Debug.Log("Inventory Loaded");
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
            Inventory newContainer = (Inventory)formatter.Deserialize(stream);
            for (int i = 0; i < GetSlots.Count; i++)
            {
                GetSlots[i].UpdateSlot(newContainer.Slots[i].item);
            }
            stream.Close();
        }
    }
    [Button]
    public void Clear()
    {
        Container.Clear();
        Container.SetAllowedItems(InvenotryAllowedItems);
    }
}
[System.Serializable]
public class Inventory: ISerializationCallbackReceiver
{
    public int inventorySize;
    public List<InventorySlot> Slots = new List<InventorySlot>();

    public void Clear()
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            Slots[i].RemoveItem();
        }
    }

    public void OnAfterDeserialize()
    {
    }

    public void OnBeforeSerialize()
    {
        if (Slots.Count != inventorySize)
        {
            Debug.Log("Trigger Serialize resize");
            Slots = new List<InventorySlot>();
            for (int i = 0; i < inventorySize; i++)
            {
                Slots.Add(new InventorySlot());
            }
        }
    }

    public void SetAllowedItems(ItemType[] _allowedItems)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            Slots[i].AllowedItems = _allowedItems;
        }
    }
}

public delegate void SlotUpdated(InventorySlot _slot);

[System.Serializable]
public class InventorySlot
{
    public ItemType[] AllowedItems = new ItemType[0];
    [System.NonSerialized]
    public InventoryInterface parent;
    [System.NonSerialized]
    public GameObject slotDisplay;
    [System.NonSerialized]
    public SlotUpdated OnAfterUpdate;
    [System.NonSerialized]
    public SlotUpdated OnBeforeUpdate;
    [System.NonSerialized]
    public SlotUpdated ItemDropped;

    public UnityAction<bool> UpdateSelected;

    public Item item = new Item();
    public bool IsEmpty { get { return item.Id == -1; } }

    public ItemObject ItemObject
    {
        get
        {
            if (item.Id >= 0)
            {
                return parent.inventory.database.ItemObjects[item.Id];
            }
            return null;
        }
    }

    public InventorySlot()
    {
        UpdateSlot(new Item());
    }
    public InventorySlot(Item _item)
    {
        UpdateSlot(_item);
    }
    public void UpdateSlot(Item _item)
    {
        OnBeforeUpdate?.Invoke(this);
        item = _item;
        item.InitialEffects = _item.InitialEffects;
        item.FollowUpEffects = _item.FollowUpEffects;
        OnAfterUpdate?.Invoke(this);
    }
    public void RemoveItem()
    {
        UpdateSlot(new Item());
    }
    public void DropItem()
    {
        ItemDropped?.Invoke(this);
        UpdateSlot(new Item());
    }
    public bool CanPlaceInSlot(ItemObject _itemObject)
    {
        if (AllowedItems.Length <= 0 || _itemObject == null || _itemObject.data.Id < 0)
            return true;
        for (int i = 0; i < AllowedItems.Length; i++)
        {
            if (_itemObject.data.Type == AllowedItems[i])
                return true;
        }
        return false;
    }
}
