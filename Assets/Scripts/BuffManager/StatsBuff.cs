using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stats Buff", menuName = "Buffs/New Stats Buff")]
public class StatsBuff : StatusEffectObject
{
    public float speedDeltaRatio;
    public float speedDeltaFlat;
    public int maxHealthDelta;
    public float critRateDelta;
    public float critDamageDelta;
    public int damageDeltaFlat;
    public float damgeDeltaRatio;

    public override StatusEffectManager InitializeBuff(GameObject obj)
    {
        return new StatsBuffManager(this, obj);
    }
}
