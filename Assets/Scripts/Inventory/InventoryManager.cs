using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

public class InventoryManager : MonoBehaviour
{
    public int inventorySize;
    private int currentInventoryCount;

    public int WeaponLeftSize;
    private int currentWeaponLeftCount;
    public int WeaponRightSize;
    private int currentWeaponRightCount;

    public GameObject slot;
    public GameObject inventoryGrid;
    public GameObject weaponLeftGrid;
    public GameObject weaponRightGrid;
    public GameObject inventoryObject;
    public GameObject bagBtn;
    public GameObject leftReload;
    public GameObject rightReload;
    public TMP_Text weaponPosText;

    [SerializeField]
    private GameObject[] inventory;
    [SerializeField]
    private GameObject[] weaponLeft;
    [SerializeField]
    private GameObject[] weaponRight;

    public GameObject player;
    private WeaponManager wm;

    void Awake()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            Instantiate(slot, inventoryGrid.transform);
        }

        for (int i = 0; i < WeaponLeftSize; i++)
        {
            Instantiate(slot, weaponLeftGrid.transform);
        }

        for (int i = 0; i < WeaponRightSize; i++)
        {
            Instantiate(slot, weaponRightGrid.transform);
        }
        inventory = new GameObject[inventorySize];
        weaponLeft = new GameObject[WeaponLeftSize];
        weaponRight = new GameObject[WeaponRightSize];
        leftReload.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64 + (WeaponLeftSize - 1) * 68);
        rightReload.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64 + (WeaponRightSize - 1) * 68);
        leftReload.GetComponent<Image>().fillAmount = 0;
        rightReload.GetComponent<Image>().fillAmount = 0;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        wm = player.GetComponent<WeaponManager>();
    }

    void Update()
    {
        //ContentUpdate();
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
        leftReload.GetComponent<Image>().fillAmount =
            player.GetComponent<WeaponManager>().reloadTimerLeft
            / player.GetComponent<WeaponManager>().totalReloadLeft;
        rightReload.GetComponent<Image>().fillAmount =
            player.GetComponent<WeaponManager>().reloadTimerRight
            / player.GetComponent<WeaponManager>().totalReloadRight;

        switch (player.GetComponent<WeaponManager>().currentPos)
        {
            case WeaponManager.SwordPosition.left:
                weaponPosText.text = "L";
                break;
            case WeaponManager.SwordPosition.right:
                weaponPosText.text = "R";
                break;
            case WeaponManager.SwordPosition.up:
                weaponPosText.text = "U";
                break;
            case WeaponManager.SwordPosition.down:
                weaponPosText.text = "D";
                break;
            case WeaponManager.SwordPosition.front:
                weaponPosText.text = "F";
                break;
            case WeaponManager.SwordPosition.back:
                weaponPosText.text = "B";
                break;
            case WeaponManager.SwordPosition.sheathed:
                weaponPosText.text = "S";
                break;
            case WeaponManager.SwordPosition.any:
                weaponPosText.text = "A";
                break;
        }

    }

    public bool AddItem(GameObject itemObject)
    {
        if (currentInventoryCount < inventorySize)
        {
            for (int i = 0; i < inventoryGrid.transform.childCount; i++)
            {
                if (!inventoryGrid.transform.GetChild(i).GetComponent<InventorySlot>().hasItem)
                {
                    currentInventoryCount++;
                    itemObject.transform.SetParent(inventoryGrid.transform.GetChild(i).Find("ItemSlot"));
                    itemObject.transform.SetAsFirstSibling();
                    inventoryGrid.transform.GetChild(i).GetComponent<InventorySlot>().hasItem = true;
                    break;
                }
            }
            return true;
        }
        else if (itemObject.GetComponent<ItemController>().thisItem.IsWeaponPart
            && currentWeaponLeftCount < WeaponLeftSize)
        {
            for (int i = 0; i < inventoryGrid.transform.childCount; i++)
            {
                if (!weaponLeftGrid.transform.GetChild(i).GetComponent<InventorySlot>().hasItem)
                {
                    currentWeaponLeftCount++;
                    //GameObject item = Instantiate(itemObject, weaponGrid.transform.GetChild(i));
                    //item.transform.SetAsFirstSibling();
                    itemObject.transform.SetParent(weaponLeftGrid.transform.GetChild(i).Find("ItemSlot"));
                    itemObject.transform.SetAsFirstSibling();
                    break;
                }
            }
            return true;
        }
        else if (itemObject.GetComponent<ItemController>().thisItem.IsWeaponPart
            && currentWeaponRightCount < WeaponRightSize)
        {
            for (int i = 0; i < inventoryGrid.transform.childCount; i++)
            {
                if (!weaponRightGrid.transform.GetChild(i).GetComponent<InventorySlot>().hasItem)
                {
                    currentWeaponRightCount++;
                    //GameObject item = Instantiate(itemObject, weaponGrid.transform.GetChild(i));
                    //item.transform.SetAsFirstSibling();
                    itemObject.transform.SetParent(weaponRightGrid.transform.GetChild(i).Find("ItemSlot"));
                    itemObject.transform.SetAsFirstSibling();
                    break;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ContentUpdate()
    {
        int tempBagCount = 0;
        int tempWeaponLeftCount = 0;
        int tempWeaponRightCount = 0;
        GameObject[] tempInventory = new GameObject[inventorySize];
        GameObject[] tempWeaponLeft = new GameObject[WeaponLeftSize];
        GameObject[] tempWeaponRight = new GameObject[WeaponRightSize];

        //Debug.Log(inventorySize);
        //Debug.Log(inventoryGrid.transform.childCount);

        if (inventorySize > inventoryGrid.transform.childCount)
        {
            for (int i = 0; i < inventorySize - inventoryGrid.transform.childCount; i++)
            {
                Instantiate(slot, inventoryGrid.transform);
            }
        }
        else if (inventorySize < inventoryGrid.transform.childCount)
        {
            Debug.Log("to smaller");
            for (int i = inventoryGrid.transform.childCount - 1; i >= inventorySize; i--)
            {
                if (inventory[i] != null)
                {
                    if (currentInventoryCount < inventorySize)
                    {
                        GameObject tempSlot = NextAvailableInventorySlot();
                        inventory[i].transform.SetParent(tempSlot.transform.Find("ItemSlot"));
                        inventory[i].transform.SetAsFirstSibling();
                    }
                    else
                    {
                        DropItem(inventory[i]);
                        currentInventoryCount--;
                    }
                }
                Debug.Log(inventoryGrid.transform.GetChild(i).name);
                Destroy(inventoryGrid.transform.GetChild(i).gameObject);
            }
        }
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryGrid.transform.GetChild(i).GetComponent<InventorySlot>().hasItem)
            {
                tempBagCount++;
                tempInventory[i] = inventoryGrid.transform.GetChild(i).Find("ItemSlot").GetChild(0).gameObject;
                if (tempInventory[i].GetComponent<ItemController>().thisItem.ThisItemType == Item.ItemType.relic)
                {
                    foreach (Buff buff in ((Relic)tempInventory[i].GetComponent<ItemController>().thisItem).buffsApplied)
                    {
                        player.GetComponent<PlayerController>().AddBuff(buff.InitializeBuff(player));
                    }
                }
            }
            else
            {
                //Debug.Log(i);
                tempInventory[i] = null;
            }
        }

        if (WeaponLeftSize > weaponLeftGrid.transform.childCount)
        {
            Instantiate(slot, weaponLeftGrid.transform);
        }
        else if (WeaponLeftSize < weaponLeftGrid.transform.childCount)
        {
            for (int i = weaponLeftGrid.transform.childCount - 1; i >= WeaponLeftSize; i--)
            {
                if (weaponLeft[i] != null)
                {
                    if (currentInventoryCount < inventorySize)
                    {
                        GameObject tempSlot = NextAvailableInventorySlot();
                        inventory[i].transform.SetParent(tempSlot.transform.Find("ItemSlot"));
                        inventory[i].transform.SetAsFirstSibling();
                    }
                    else
                    {
                        DropItem(weaponLeft[i]);
                        currentWeaponLeftCount--;
                    }
                }
                Destroy(weaponLeftGrid.transform.GetChild(i).gameObject);
            }
        }
        int j = 0;
        for (int i = 0; i < WeaponLeftSize; i++)
        {
            if (weaponLeftGrid.transform.GetChild(i).GetComponent<InventorySlot>().hasItem)
            {
                if (j == wm.weaponPtrLeft && !wm.reloadingLeft)
                {
                    weaponLeftGrid.transform.GetChild(i).Find("Select").gameObject.SetActive(true);
                } else
                {
                    weaponLeftGrid.transform.GetChild(i).Find("Select").gameObject.SetActive(false);
                }

                tempWeaponLeftCount++;
                tempWeaponLeft[i] = weaponLeftGrid.transform.GetChild(i).Find("ItemSlot").GetChild(0).gameObject;
                j++;
            }
            else
            {
                tempWeaponLeft[i] = null;
                weaponLeftGrid.transform.GetChild(i).Find("Select").gameObject.SetActive(false);
            }
        }

        if (WeaponRightSize > weaponRightGrid.transform.childCount)
        {
            Instantiate(slot, weaponRightGrid.transform);
        }
        else if (WeaponRightSize < weaponRightGrid.transform.childCount)
        {
            for (int i = weaponRightGrid.transform.childCount - 1; i >= WeaponRightSize; i--)
            {
                if (weaponRight[i] != null)
                {
                    if (currentInventoryCount < inventorySize)
                    {
                        GameObject tempSlot = NextAvailableInventorySlot();
                        inventory[i].transform.SetParent(tempSlot.transform.Find("ItemSlot"));
                        inventory[i].transform.SetAsFirstSibling();
                    }
                    else
                    {
                        DropItem(weaponRight[i]);
                        currentWeaponRightCount--;
                    }
                }
                Destroy(weaponRightGrid.transform.GetChild(i).gameObject);
            }
        }
        j = 0;
        for (int i = 0; i < WeaponRightSize; i++)
        {
            if (weaponRightGrid.transform.GetChild(i).GetComponent<InventorySlot>().hasItem)
            {
                if (j == wm.weaponPtrRight && !wm.reloadingRight)
                {
                    weaponRightGrid.transform.GetChild(i).Find("Select").gameObject.SetActive(true);
                }
                else
                {
                    weaponRightGrid.transform.GetChild(i).Find("Select").gameObject.SetActive(false);
                }

                tempWeaponRightCount++;
                tempWeaponRight[i] = weaponRightGrid.transform.GetChild(i).Find("ItemSlot").GetChild(0).gameObject;
                j++;
            }
            else
            {
                tempWeaponRight[i] = null;
                weaponRightGrid.transform.GetChild(i).Find("Select").gameObject.SetActive(false);
            }
        }

        currentInventoryCount = tempBagCount;
        currentWeaponLeftCount = tempWeaponLeftCount;
        currentWeaponRightCount = tempWeaponRightCount;
        inventory = tempInventory;
        weaponLeft = tempWeaponLeft;
        weaponRight = tempWeaponRight;
    }

    public GameObject NextAvailableInventorySlot()
    {
        GameObject ret = null;
        for (int i = 0; i < inventoryGrid.transform.childCount; i++)
        {
            if (!inventoryGrid.transform.GetChild(i).GetComponent<InventorySlot>().hasItem)
            {
                ret = inventoryGrid.transform.GetChild(i).gameObject;
                return ret;
            }
        }
        return null;
    }

    public GameObject NextAvailableWeaponLeftSlot()
    {
        GameObject ret = null;
        for (int i = 0; i < weaponLeftGrid.transform.childCount; i++)
        {
            if (!weaponLeftGrid.transform.GetChild(i).GetComponent<InventorySlot>().hasItem)
            {
                ret = weaponLeftGrid.transform.GetChild(i).gameObject;
                return ret;
            }
        }
        return null;
    }

    public GameObject NextAvailableWeaponRightSlot()
    {
        GameObject ret = null;
        for (int i = 0; i < weaponRightGrid.transform.childCount; i++)
        {
            if (!weaponRightGrid.transform.GetChild(i).GetComponent<InventorySlot>().hasItem)
            {
                ret = weaponRightGrid.transform.GetChild(i).gameObject;
                return ret;
            }
        }
        return null;
    }

    public List<Item> GetInventory()
    {
        List<Item> ret = new List<Item>();
        foreach (GameObject i in inventory)
        {
            if (i)
            {
                ret.Add(i.GetComponent<ItemController>().thisItem);
            }
        }
        return ret;
    }

    public List<Item> GetWeaponLeft()
    {
        List<Item> ret = new List<Item>();
        foreach (GameObject i in weaponLeft)
        {
            if (i)
            {
                ret.Add(i.GetComponent<ItemController>().thisItem);
            }
        }
        return ret;
    }

    public List<Item> GetWeaponRight()
    {
        List<Item> ret = new List<Item>();
        foreach (GameObject i in weaponRight)
        {
            if (i)
            {
                ret.Add(i.GetComponent<ItemController>().thisItem);
            }
        }
        return ret;
    }

    public void ToggleInventory()
    {
        inventoryObject.SetActive(!inventoryObject.activeSelf);
        wm.canAttack = !inventoryObject.activeSelf;
        ContentUpdate();
    }

    private void DropItem(GameObject item)
    {
        item.GetComponent<ItemController>().parentAfterDrag = null;
        item.transform.SetParent(null);
        item.transform.position = player.transform.position;
    }

    //public void ChangeInventorySize(int num)
    //{
    //    if (num == 0) return;
    //    else if (num > 0)
    //    {
    //        for (int i = 0; i < num; i++)
    //        {
    //            Instantiate(slot, inventoryGrid.transform);
    //        }
    //        inventorySize += num;
    //    } else
    //    {

    //    }
    //}

    [Button]
    public void ChangeInventorySize()
    {
        inventorySize--;
        ContentUpdate();
    }

    [Button]
    public void ChangeWLSize()
    {
        WeaponLeftSize--;
        ContentUpdate();
    }

    [Button]
    public void ChangeWRSize()
    {
        WeaponRightSize--;
        ContentUpdate();
    }

    [Button]
    public void AddInventorySize()
    {
        inventorySize++;
        ContentUpdate();
    }

    [Button]
    public void AddWLSize()
    {
        WeaponLeftSize++;
        ContentUpdate();
    }

    [Button]
    public void AddWRSize()
    {
        WeaponRightSize++;
        ContentUpdate();
    }
}
