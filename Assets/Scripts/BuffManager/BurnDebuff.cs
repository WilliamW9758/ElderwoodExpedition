using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Burn Debuff", menuName = "Buffs/New Burn debuff")]
public class BurnDebuff : Buff
{
    public int burnDamage;
    public float tickRate;

    public override BuffManager InitializeBuff(GameObject obj)
    {
        return new BurnDebuffManager(this, obj);
    }
}
