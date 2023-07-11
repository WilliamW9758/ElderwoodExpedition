using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fire Buff", menuName = "Buffs/New Fire Buff")]
public class FireBuff : Buff
{
    public int flatDamageDelta;
    public Buff burnDebuff;

    public override BuffManager InitializeBuff(GameObject obj)
    {
        return new FireBuffManager(this, obj);
    }
}
