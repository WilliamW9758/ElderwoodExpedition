using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Relic", menuName = "Inventory/New Relic")]
public class Relic : Item
{
    public List<Buff> buffsApplied;
}
