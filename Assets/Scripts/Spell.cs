using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Spell", menuName = "Inventory/New Spell")]
public class Spell : Item
{
    public GameObject effect; // the special effect game object that gets instantiated
    public float energyDelta; // gain/cost energy when hit/cast
    public List<Buff> buffsApplied;
}
