using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Speed Buff", menuName = "Buffs/New Speed Buff")]
public class SpeedBuff : Buff
{
    public float speedDeltaRatio;
    public float speedDeltaFlat;

    public override BuffManager InitializeBuff(GameObject obj)
    {
        return new SpeedBuffManager(this, obj);
    }
}
