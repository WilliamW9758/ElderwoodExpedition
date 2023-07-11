using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Attack", menuName = "Inventory/New Attack")]
public class Attack : Item
{
    public GameObject effect; // the special effect game object that gets instantiated
    public Vector2 moveVec; // movement when attacking, relative to UP direction

    public int damage; // flat damage
    public int knockback; // knockback relative to player location
    public float knockup; // airborne enemy but doesn't knockback
    public int stunAmount; // the amount of weakness break
    public float energyDelta; // gain/cost energy when hit/cast
    public float specialEnergyDelta; // gain/cost energy if special effect trigger, e.g. success counter

    public WeaponManager.SwordPosition startPos;
    public WeaponManager.SwordPosition endPos;
    public int swordStateID;
    public int playerStateID;
}
