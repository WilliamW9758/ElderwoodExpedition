using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New On Hit Effect Buff", menuName = "Buffs/New On Hit Effect Buff")]
public class OnHitEffectBuff : StatusEffectObject
{
    public StatusEffectObject onHitApplyDebuff;
    public Elements elementInfuse;

    public override StatusEffectManager InitializeBuff(GameObject obj)
    {
        return new OnHitEffectBuffManager(this, obj);
    }
}
