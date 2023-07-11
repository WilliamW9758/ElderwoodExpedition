using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/New Item")]
public class Item : ScriptableObject
{
    public string ItemName = "";
    public Sprite ItemWorldImage = null;
    public Sprite ItemInventoryImage = null;
    [TextArea]
    public string ItemInfo = "";

    public bool Usable = false;
    public bool IsWeaponPart = false;
    public enum ItemType
    {
        none,
        relic,
        attack,
        spell,
        combo,
    }
    public ItemType ThisItemType = ItemType.none;
}
