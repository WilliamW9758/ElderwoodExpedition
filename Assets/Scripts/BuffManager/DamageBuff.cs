using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Buff", menuName = "Buffs/New Damage Buff")]
public class DamageBuff : Buff
{
    public float damageDeltaRatio;
    public int damageDeltaFlat;

    public override BuffManager InitializeBuff(GameObject obj)
    {
        return new DamageBuffManager(this, obj);
    }
}
